'
' DotNetNuke� - http://www.dotnetnuke.com
' Copyright (c) 2002-2009
' by DotNetNuke Corporation
'
' Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
' documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
' the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
' to permit persons to whom the Software is furnished to do so, subject to the following conditions:
'
' The above copyright notice and this permission notice shall be included in all copies or substantial portions 
' of the Software.
'
' THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
' TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
' THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
' CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
' DEALINGS IN THE SOFTWARE.
'

Imports System
Imports System.Collections.Generic
Imports System.Data

Imports DotNetNuke.Entities.Modules
Imports DotNetNuke.Entities.Portals
Imports System.Xml
Imports DotNetNuke.Security.Roles
Imports DotNetNuke.Security.Permissions
Imports System.Threading
Imports DotNetNuke.Entities.Content
Imports DotNetNuke.Entities.Content.Taxonomy

Namespace DotNetNuke.Entities.Tabs

    Public Class TabController

        Private Shared provider As DataProvider = DataProvider.Instance()

        Public Shared ReadOnly Property CurrentPage() As TabInfo
            Get
                Dim _tab As TabInfo = Nothing
                If PortalController.GetCurrentPortalSettings IsNot Nothing Then
                    _tab = PortalController.GetCurrentPortalSettings.ActiveTab
                End If
                Return _tab
            End Get
        End Property

#Region "Private Methods"

        Private Sub AddAllTabsModules(ByVal objTab As TabInfo)
            Dim objmodules As New ModuleController

            For Each allTabsModule As ModuleInfo In objmodules.GetAllTabsModules(objTab.PortalID, True)
                '[DNN-6276]We need to check that the Module is not implicitly deleted.  ie If all instances are on Pages
                'that are all "deleted" then even if the Module itself is not deleted, we would not expect the 
                'Module to be added
                Dim canAdd As Boolean = False
                For Each allTabsInstance As ModuleInfo In objmodules.GetModuleTabs(allTabsModule.ModuleID)
                    'Get Tab
                    Dim tab As TabInfo = New TabController().GetTab(allTabsInstance.TabID, objTab.PortalID, False)
                    If Not tab.IsDeleted Then
                        canAdd = True
                        Exit For
                    End If
                Next

                If canAdd Then
                    objmodules.CopyModule(allTabsModule.ModuleID, allTabsModule.TabID, objTab.TabID, "", True)
                End If
            Next
        End Sub

        ''' -----------------------------------------------------------------------------
        ''' <summary>
        ''' Adds a tab to the Datastore
        ''' </summary>
        ''' <param name="objTab">The tab to be added</param>
        ''' <history>
        ''' 	[cnurse]	04/30/2008	Created
        ''' 	[jlucarino]	02/26/2009	added CreatedByUserID
        ''' </history>
        ''' -----------------------------------------------------------------------------
        Private Function AddTabInternal(ByVal objTab As TabInfo, ByVal includeAllTabsModules As Boolean) As Integer
            Dim newTab As Boolean = True
            objTab.TabPath = GenerateTabPath(objTab.ParentId, objTab.TabName)
            Dim iTabID As Integer = GetTabByTabPath(objTab.PortalID, objTab.TabPath)

            If iTabID > Null.NullInteger Then
                'Tab exists so Throw
                Throw New TabExistsException(iTabID, "Tab Exists")
            Else
                'First create ContentItem as we need the ContentItemID
                Dim typeController As IContentTypeController = New ContentTypeController
                Dim contentType As ContentType = (From t In typeController.GetContentTypes() _
                                                 Where t.ContentType = "Tab" _
                                                 Select t) _
                                                 .SingleOrDefault

                Dim contentController As IContentController = DotNetNuke.Entities.Content.Common.GetContentController()
                If String.IsNullOrEmpty(objTab.Title) Then
                    objTab.Content = objTab.TabName
                Else
                    objTab.Content = objTab.Title
                End If
                objTab.ContentTypeId = contentType.ContentTypeId
                objTab.Indexed = False
                Dim contentItemID As Integer = contentController.AddContentItem(objTab)

                'Add Module
                iTabID = provider.AddTab(contentItemID, objTab.PortalID, objTab.TabName, objTab.IsVisible, objTab.DisableLink, objTab.ParentId, _
                         objTab.IconFile, objTab.IconFileLarge, objTab.Title, objTab.Description, objTab.KeyWords, objTab.Url, _
                         objTab.SkinSrc, objTab.ContainerSrc, objTab.TabPath, objTab.StartDate, objTab.EndDate, _
                         objTab.RefreshInterval, objTab.PageHeadText, objTab.IsSecure, objTab.PermanentRedirect, _
                         objTab.SiteMapPriority, UserController.GetCurrentUserInfo.UserID, Entities.Host.Host.ContentLocale.ToString)

                objTab.TabID = iTabID

                'Now we have the TabID - update the contentItem
                contentController.UpdateContentItem(objTab)

                Dim termController As ITermController = DotNetNuke.Entities.Content.Common.GetTermController()
                termController.RemoveTermsFromContent(objTab)
                For Each _Term As Term In objTab.Terms
                    termController.AddTermToContent(_Term, objTab)
                Next

                Dim objEventLog As New Services.Log.EventLog.EventLogController
                objEventLog.AddLog(objTab, PortalController.GetCurrentPortalSettings, UserController.GetCurrentUserInfo.UserID, "", Services.Log.EventLog.EventLogController.EventLogType.TAB_CREATED)

                'Add Tab Permissions
                TabPermissionController.SaveTabPermissions(objTab)

                'Add TabSettings - use Try/catch as tabs are added during upgrade ptocess and the sproc may not exist
                Try
                    UpdateTabSettings(objTab)
                Catch ex As Exception
                    LogException(ex)
                End Try

                'Add AllTabs Modules
                If includeAllTabsModules Then
                    AddAllTabsModules(objTab)
                End If
            End If

            Return objTab.TabID
        End Function

        ''' -----------------------------------------------------------------------------
        ''' <summary>
        ''' Adds a tab to the end of a List
        ''' </summary>
        ''' <param name="objTab">The tab to be added</param>
        ''' <param name="updateTabPath">A flag that indicates whether the TabPath is updated.</param>
        ''' <history>
        ''' 	[cnurse]	04/30/2008	Created
        ''' </history>
        ''' -----------------------------------------------------------------------------
        Private Sub AddTabToEndOfList(ByVal objTab As TabInfo, ByVal updateTabPath As Boolean)
            'Get the List of tabs with the same parent
            Dim siblingTabs As List(Of TabInfo) = GetTabsByPortal(objTab.PortalID).WithParentId(objTab.ParentId)

            'Get the Parent Tab
            Dim parentTab As TabInfo = GetTab(objTab.ParentId, objTab.PortalID, False)
            If parentTab Is Nothing Then
                objTab.Level = 0
            Else
                objTab.Level = parentTab.Level + 1
            End If

            'Update the TabOrder for the Siblings
            UpdateTabOrder(siblingTabs, 2)

            objTab.TabOrder = 2 * siblingTabs.Count + 1

            'UpdateOrder 
            UpdateTabOrder(objTab, updateTabPath)
        End Sub

        Private Sub DeleteTabInternal(ByVal tabId As Integer, ByVal portalId As Integer)
            'Get the tab from the Cache
            Dim objTab As TabInfo = GetTab(tabId, portalId, False)

            'Delete Tab
            provider.DeleteTab(tabId)

            'Remove the Content Item
            If objTab.ContentItemId > Null.NullInteger Then
                Dim ctl As IContentController = DotNetNuke.Entities.Content.Common.GetContentController()
                ctl.DeleteContentItem(objTab)
            End If

            'Log deletion
            Dim objEventLog As New Services.Log.EventLog.EventLogController
            objEventLog.AddLog("TabID", tabId.ToString, PortalController.GetCurrentPortalSettings, UserController.GetCurrentUserInfo.UserID, Services.Log.EventLog.EventLogController.EventLogType.TAB_DELETED)
        End Sub

        Private Sub DemoteTab(ByVal objTab As TabInfo, ByVal siblingTabs As List(Of TabInfo))
            Dim siblingCount As Integer = siblingTabs.Count

            'Get Tab's Index position in the Sibling List
            Dim tabIndex As Integer = GetIndexOfTab(objTab, siblingTabs)

            'Cannot demote tab that is the first sibling
            If tabIndex > 0 Then
                'All the siblings move up in the order
                UpdateTabOrder(siblingTabs, tabIndex + 1, siblingCount - 1, -2)

                'New parent is tab with index of tabIndex -1
                Dim parentTab As TabInfo = siblingTabs(tabIndex - 1)

                'Get the descendents now before the parent is updated
                Dim descendantTabs As List(Of TabInfo) = GetTabsByPortal(objTab.PortalID).DescendentsOf(objTab.TabID)

                'Update the current tab and add to the end of the new parents list of children
                objTab.ParentId = parentTab.TabID
                AddTabToEndOfList(objTab, True)

                'Update the Descendents of this tab
                UpdateDescendantLevel(descendantTabs, 1)
            End If
        End Sub

        Private Function GetTabByNameAndParent(ByVal TabName As String, ByVal PortalId As Integer, ByVal ParentId As Integer) As TabInfo
            Dim arrTabs As ArrayList = GetTabsByNameAndPortal(TabName, PortalId)
            Dim intTab As Integer = -1

            If Not arrTabs Is Nothing Then
                Select Case arrTabs.Count
                    Case 0    ' no results
                    Case 1    ' exact match
                        intTab = 0
                    Case Else    ' multiple matches
                        Dim intIndex As Integer
                        Dim objTab As TabInfo
                        For intIndex = 0 To arrTabs.Count - 1
                            objTab = CType(arrTabs(intIndex), TabInfo)
                            ' check if the parentids match
                            If objTab.ParentId = ParentId Then
                                intTab = intIndex
                            End If
                        Next intIndex
                        If intTab = -1 Then
                            ' no match - return the first item
                            intTab = 0
                        End If
                End Select
            End If

            If intTab <> -1 Then
                Return CType(arrTabs(intTab), TabInfo)
            Else
                Return Nothing
            End If
        End Function

        Private Function GetTabsByNameAndPortal(ByVal TabName As String, ByVal PortalId As Integer) As ArrayList
            Dim returnTabs As New ArrayList()
            For Each kvp As KeyValuePair(Of Integer, TabInfo) In GetTabsByPortal(PortalId)
                Dim objTab As TabInfo = kvp.Value
                If String.Compare(objTab.TabName, TabName, True) = 0 Then
                    returnTabs.Add(objTab)
                End If
            Next
            Return returnTabs
        End Function

        Private Function GetIndexOfTab(ByVal objTab As TabInfo, ByVal tabs As List(Of TabInfo)) As Integer
            Dim tabIndex As Integer = Null.NullInteger
            For index As Integer = 0 To tabs.Count - 1
                If tabs(index).TabID = objTab.TabID Then
                    tabIndex = index
                    Exit For
                End If
            Next
            Return tabIndex
        End Function

        Private Sub PromoteTab(ByVal objTab As TabInfo, ByVal siblingTabs As List(Of TabInfo))
            Dim siblingCount As Integer = siblingTabs.Count

            'Get the Parent Tab (we need to know the current position of the parent in)
            Dim parentTab As TabInfo = GetTab(objTab.ParentId, objTab.PortalID, False)

            If parentTab IsNot Nothing Then
                'Get Tab's Index position in the Sibling List
                Dim tabIndex As Integer = GetIndexOfTab(objTab, siblingTabs)

                'All the siblings move up in the order
                UpdateTabOrder(siblingTabs, tabIndex + 1, siblingCount - 1, -2)

                'Get the siblings of the Parent
                siblingTabs = GetTabsByPortal(objTab.PortalID).WithParentId(parentTab.ParentId)
                siblingCount = siblingTabs.Count

                'First make sure the list is sorted and spaced
                UpdateTabOrder(siblingTabs, 2)

                'Get Parents Index position in the Sibling List
                Dim parentIndex As Integer = GetIndexOfTab(parentTab, siblingTabs)

                'We need to update the taborder for items that were after the parent
                UpdateTabOrder(siblingTabs, parentIndex + 1, siblingCount - 1, 2)

                'Get the descendents now before the parent is updated
                Dim descendantTabs As List(Of TabInfo) = GetTabsByPortal(objTab.PortalID).DescendentsOf(objTab.TabID)

                'Update the current Tab
                objTab.ParentId = parentTab.ParentId
                objTab.TabOrder = parentTab.TabOrder + 2
                UpdateTab(objTab)

                'Update the current tabs level and tabpath
                objTab.Level = objTab.Level - 1
                UpdateTabOrder(objTab, True)

                'Update the Descendents of this tab
                UpdateDescendantLevel(descendantTabs, -1)
            End If
        End Sub

        ''' -----------------------------------------------------------------------------
        ''' <summary>
        ''' Removes a tab from its current siblings
        ''' </summary>
        ''' <param name="objTab">Tab to remove</param>
        ''' <history>
        ''' 	[cnurse]	04/30/2008	Created
        ''' </history>
        ''' -----------------------------------------------------------------------------
        Private Sub RemoveTab(ByVal objTab As TabInfo)
            'Tab is being moved from the original list of siblings, so update the Taborder for the remaining tabs
            Dim siblingTabs As List(Of TabInfo) = GetTabsByPortal(objTab.PortalID).WithParentId(objTab.ParentId)
            Dim siblingCount As Integer = siblingTabs.Count
            For index As Integer = 0 To siblingCount - 1
                Dim sibling As TabInfo = siblingTabs(index)
                If sibling.TabID = objTab.TabID Then
                    'This is the tab we are moving so update the taborder for the remaining items
                    UpdateTabOrder(siblingTabs, index + 1, siblingCount - 1, -2)

                    Exit For
                End If
            Next
        End Sub

        ''' -----------------------------------------------------------------------------
        ''' <summary>
        ''' Swaps two adjacent tabs in the same level
        ''' </summary>
        ''' <param name="firstTab">ID of the first tab</param>
        ''' <param name="secondTab">ID of the second Tab</param>
        ''' <history>
        ''' 	[cnurse]	04/30/2008	Created
        ''' </history>
        ''' -----------------------------------------------------------------------------
        Private Sub SwapAdjacentTabs(ByVal firstTab As TabInfo, ByVal secondTab As TabInfo)
            firstTab.TabOrder -= 2

            'UpdateOrder - Parent hasn't changed so we don't need to regenerate TabPath
            UpdateTabOrder(firstTab, False)

            secondTab.TabOrder += 2

            'UpdateOrder - Parent hasn't changed so we don't need to regenerate TabPath
            UpdateTabOrder(secondTab, False)
        End Sub

        ''' -----------------------------------------------------------------------------
        ''' <summary>
        ''' Updates child tabs TabPath field
        ''' </summary>
        ''' <param name="intTabid">ID of the parent tab</param>
        ''' <remarks>
        ''' When a ParentTab is updated this method should be called to 
        ''' ensure that the TabPath of the Child Tabs is consistent with the Parent
        ''' </remarks>
        ''' <history>
        ''' 	[JWhite]	16/11/2004	Created
        ''' </history>
        ''' -----------------------------------------------------------------------------
        Private Sub UpdateChildTabPath(ByVal intTabid As Integer, ByVal portalId As Integer)
            For Each objtab As TabInfo In GetTabsByPortal(portalId).DescendentsOf(intTabid)
                Dim oldTabPath As String = objtab.TabPath
                objtab.TabPath = GenerateTabPath(objtab.ParentId, objtab.TabName)
                If oldTabPath <> objtab.TabPath Then
                    provider.UpdateTab(objtab.TabID, objtab.ContentItemId, objtab.PortalID, objtab.TabName, objtab.IsVisible, objtab.DisableLink, _
                           objtab.ParentId, objtab.IconFile, objtab.IconFileLarge, objtab.Title, objtab.Description, _
                           objtab.KeyWords, objtab.IsDeleted, objtab.Url, objtab.SkinSrc, objtab.ContainerSrc, _
                           objtab.TabPath, objtab.StartDate, objtab.EndDate, objtab.RefreshInterval, _
                           objtab.PageHeadText, objtab.IsSecure, objtab.PermanentRedirect, _
                           objtab.SiteMapPriority, UserController.GetCurrentUserInfo.UserID, Entities.Host.Host.ContentLocale.ToString)
                    Dim objEventLog As New Services.Log.EventLog.EventLogController
                    objEventLog.AddLog("TabID", intTabid.ToString, PortalController.GetCurrentPortalSettings, UserController.GetCurrentUserInfo.UserID, Services.Log.EventLog.EventLogController.EventLogType.TAB_UPDATED)

                End If
            Next
        End Sub

        Private Sub UpdateDescendantLevel(ByVal descendantTabs As List(Of TabInfo), ByVal levelDelta As Integer)
            'Update the Descendents of this tab
            For Each descendent As TabInfo In descendantTabs
                descendent.Level = descendent.Level + levelDelta
                UpdateTabOrder(descendent, True)
            Next
        End Sub

        ''' -----------------------------------------------------------------------------
        ''' <summary>
        ''' Updates the TabOrder for a single Tab
        ''' </summary>
        ''' <param name="objTab">The tab to be updated</param>
        ''' <param name="updateTabPath">A flag that indicates whether the TabPath is updated.</param>
        ''' <history>
        ''' 	[cnurse]	04/30/2008	Created
        ''' </history>
        ''' -----------------------------------------------------------------------------
        Private Sub UpdateTabOrder(ByVal objTab As TabInfo, ByVal updateTabPath As Boolean)
            If updateTabPath Then
                objTab.TabPath = GenerateTabPath(objTab.ParentId, objTab.TabName)
            End If
            provider.UpdateTabOrder(objTab.TabID, objTab.TabOrder, objTab.Level, objTab.ParentId, objTab.TabPath, UserController.GetCurrentUserInfo.UserID)
            Dim objEventLog As New Services.Log.EventLog.EventLogController
            objEventLog.AddLog(objTab, PortalController.GetCurrentPortalSettings, UserController.GetCurrentUserInfo.UserID, "", Services.Log.EventLog.EventLogController.EventLogType.TAB_ORDER_UPDATED)
        End Sub

        ''' -----------------------------------------------------------------------------
        ''' <summary>
        ''' Updates the TabOrder for a list of Tabs
        ''' </summary>
        ''' <param name="tabs">The List of tabs to be updated</param>
        ''' <param name="startIndex">The index to start updating from</param>
        ''' <param name="increment">The increment to update each tabs TabOrder</param>
        ''' <history>
        ''' 	[cnurse]	04/30/2008	Created
        ''' </history>
        ''' -----------------------------------------------------------------------------
        Private Sub UpdateTabOrder(ByVal tabs As List(Of TabInfo), ByVal startIndex As Integer, ByVal endIndex As Integer, ByVal increment As Integer)
            For index As Integer = startIndex To endIndex
                Dim objTab As TabInfo = tabs(index)
                objTab.TabOrder += increment

                'UpdateOrder - Parent hasn't changed so we don't need to regenerate TabPath
                UpdateTabOrder(objTab, False)
            Next

        End Sub

        ''' -----------------------------------------------------------------------------
        ''' <summary>
        ''' Updates the TabOrder for a list of Tabs
        ''' </summary>
        ''' <param name="tabs">The List of tabs to be updated</param>
        ''' <param name="increment">The increment to update each tabs TabOrder</param>
        ''' <history>
        ''' 	[cnurse]	06/09/2009	Created
        ''' </history>
        ''' -----------------------------------------------------------------------------
        Private Sub UpdateTabOrder(ByVal tabs As List(Of TabInfo), ByVal increment As Integer)
            Dim tabOrder As Integer = 1
            For index As Integer = 0 To tabs.Count - 1
                Dim objTab As TabInfo = tabs(index)
                objTab.TabOrder = tabOrder

                'UpdateOrder - Parent hasn't changed so we don't need to regenerate TabPath
                UpdateTabOrder(objTab, False)

                tabOrder += increment
            Next
        End Sub

#End Region

#Region "Public Methods"

        ''' -----------------------------------------------------------------------------
        ''' <summary>
        ''' Adds a tab
        ''' </summary>
        ''' <param name="objTab">The tab to be added</param>
        ''' <remarks>The tab is added to the end of the current Level.</remarks>
        ''' <history>
        ''' 	[cnurse]	04/30/2008	Documented
        ''' </history>
        ''' -----------------------------------------------------------------------------
        Public Function AddTab(ByVal objTab As TabInfo) As Integer
            Return AddTab(objTab, True)
        End Function

        ''' -----------------------------------------------------------------------------
        ''' <summary>
        ''' Adds a tab
        ''' </summary>
        ''' <param name="objTab">The tab to be added</param>
        ''' <param name="includeAllTabsModules">Flag that indicates whether to add the "AllTabs"
        ''' Modules</param>
        ''' <remarks>The tab is added to the end of the current Level.</remarks>
        ''' <history>
        ''' 	[cnurse]	04/30/2008	Documented
        ''' </history>
        ''' -----------------------------------------------------------------------------
        Public Function AddTab(ByVal objTab As TabInfo, ByVal includeAllTabsModules As Boolean) As Integer
            'Add tab to store
            Dim tabID As Integer = AddTabInternal(objTab, includeAllTabsModules)

            'UpdateOrder - Parent hasn't changed so we don't need to regenerate TabPath
            AddTabToEndOfList(objTab, False)

            'Clear the Cache
            ClearCache(objTab.PortalID)
            DataCache.RemoveCache(DataCache.PortalDictionaryCacheKey)

            Return tabID
        End Function

        ''' -----------------------------------------------------------------------------
        ''' <summary>
        ''' Adds a tab after the specified tab
        ''' </summary>
        ''' <param name="objTab">The tab to be added</param>
        ''' <param name="afterTabId">Id of the tab after which this tab is added</param>
        ''' <history>
        ''' 	[cnurse]	04/30/2008	Created
        ''' </history>
        ''' -----------------------------------------------------------------------------
        Public Function AddTabAfter(ByVal objTab As TabInfo, ByVal afterTabId As Integer) As Integer
            'Get the List of tabs with the same parent
            Dim siblingTabs As List(Of TabInfo) = GetTabsByPortal(objTab.PortalID).WithParentId(objTab.ParentId)
            Dim siblingCount As Integer = siblingTabs.Count

            'First make sure that the siblings are sorted correctly
            UpdateTabOrder(siblingTabs, 2)

            'Add tab to store
            Dim tabID As Integer = AddTabInternal(objTab, True)

            'New tab is to be inserted into the siblings List after TabId=afterTabId
            For index As Integer = 0 To siblingCount - 1
                Dim sibling As TabInfo = siblingTabs(index)
                If sibling.TabID = afterTabId Then
                    'we need to insert the tab here
                    objTab.Level = sibling.Level
                    objTab.TabOrder = sibling.TabOrder + 2

                    'UpdateOrder - Parent hasn't changed so we don't need to regenerate TabPath
                    UpdateTabOrder(objTab, False)

                    'We need to update the taborder for the remaining items
                    UpdateTabOrder(siblingTabs, index + 1, siblingCount - 1, 2)

                    Exit For
                End If
            Next

            'Clear the Cache
            ClearCache(objTab.PortalID)

            Return tabID
        End Function

        Public Sub MoveTabAfter(ByVal objTab As TabInfo, ByVal afterTabId As Integer)
            If (objTab.TabID < 0) Then
                Return
            End If

            'Get the List of tabs with the same parent
            Dim siblingTabs As List(Of TabInfo) = GetTabsByPortal(objTab.PortalID).WithParentId(objTab.ParentId)
            Dim siblingCount As Integer = siblingTabs.Count

            'First make sure that the siblings are sorted correctly
            UpdateTabOrder(siblingTabs, 2)

            'New tab is to be inserted into the siblings List after TabId=afterTabId
            For index As Integer = 0 To siblingCount - 1
                Dim sibling As TabInfo = siblingTabs(index)
                If sibling.TabID = afterTabId Then
                    'we need to insert the tab here
                    objTab.Level = sibling.Level
                    objTab.TabOrder = sibling.TabOrder + 2

                    'UpdateOrder - Parent hasn't changed so we don't need to regenerate TabPath
                    UpdateTabOrder(objTab, False)

                    'We need to update the taborder for the remaining items, excluding the current tab
                    'UpdateTabOrder(siblingTabs, index + 1, siblingCount - 1, 2)
                    Dim remainingTabOrder As Integer = objTab.TabOrder
                    For remainingIndex As Integer = index + 1 To siblingCount - 1
                        Dim remainingTab As TabInfo = siblingTabs(remainingIndex)

                        If (remainingTab.TabID = objTab.TabID) Then
                            Continue For
                        End If
                        remainingTabOrder = remainingTabOrder + 2
                        remainingTab.TabOrder = remainingTabOrder

                        'UpdateOrder - Parent hasn't changed so we don't need to regenerate TabPath
                        UpdateTabOrder(remainingTab, False)
                    Next
                    Exit For
                End If
            Next

            'Clear the Cache
            ClearCache(objTab.PortalID)
        End Sub

        Public Sub MoveTabBefore(ByVal objTab As TabInfo, ByVal beforeTabId As Integer)
            If (objTab.TabID < 0) Then
                Return
            End If

            'Get the List of tabs with the same parent
            Dim siblingTabs As List(Of TabInfo) = GetTabsByPortal(objTab.PortalID).WithParentId(objTab.ParentId)
            Dim siblingCount As Integer = siblingTabs.Count

            'First make sure that the siblings are sorted correctly
            UpdateTabOrder(siblingTabs, 2)

            'New tab is to be inserted into the siblings List before TabId=beforeTabId
            For index As Integer = 0 To siblingCount - 1
                Dim sibling As TabInfo = siblingTabs(index)
                If sibling.TabID = beforeTabId Then
                    'we need to insert the tab here
                    objTab.Level = sibling.Level
                    objTab.TabOrder = sibling.TabOrder

                    'UpdateOrder - Parent hasn't changed so we don't need to regenerate TabPath
                    UpdateTabOrder(objTab, False)

                    'We need to update the taborder for the remaining items, including the current one
                    Dim remainingTabOrder As Integer = objTab.TabOrder
                    For remainingIndex As Integer = index To siblingCount - 1
                        Dim remainingTab As TabInfo = siblingTabs(remainingIndex)

                        If (remainingTab.TabID = objTab.TabID) Then
                            Continue For
                        End If

                        remainingTabOrder = remainingTabOrder + 2
                        remainingTab.TabOrder = remainingTabOrder

                        'UpdateOrder - Parent hasn't changed so we don't need to regenerate TabPath
                        UpdateTabOrder(remainingTab, False)
                    Next
                    Exit For
                End If
            Next

            'Clear the Cache
            ClearCache(objTab.PortalID)
        End Sub

        ''' -----------------------------------------------------------------------------
        ''' <summary>
        ''' Adds a tab before the specified tab
        ''' </summary>
        ''' <param name="objTab">The tab to be added</param>
        ''' <param name="beforeTabId">Id of the tab before which this tab is added</param>
        ''' <history>
        ''' 	[cnurse]	04/30/2008	Created
        ''' </history>
        ''' -----------------------------------------------------------------------------
        Public Function AddTabBefore(ByVal objTab As TabInfo, ByVal beforeTabId As Integer) As Integer
            'Get the List of tabs with the same parent
            Dim siblingTabs As List(Of TabInfo) = GetTabsByPortal(objTab.PortalID).WithParentId(objTab.ParentId)
            Dim siblingCount As Integer = siblingTabs.Count

            'First make sure that the siblings are sorted correctly
            UpdateTabOrder(siblingTabs, 2)

            'Add tab to store
            Dim tabID As Integer = AddTabInternal(objTab, True)

            'New tab is to be inserted into the siblings List before TabId=beforeTabId
            For index As Integer = 0 To siblingCount - 1
                Dim sibling As TabInfo = siblingTabs(index)
                If sibling.TabID = beforeTabId Then
                    'we need to insert the tab here
                    objTab.Level = sibling.Level
                    objTab.TabOrder = sibling.TabOrder

                    'UpdateOrder - Parent hasn't changed so we don't need to regenerate TabPath
                    UpdateTabOrder(objTab, False)

                    'We need to update the taborder for the remaining items
                    UpdateTabOrder(siblingTabs, index, siblingCount - 1, 2)

                    Exit For
                End If
            Next

            'Clear the Cache
            ClearCache(objTab.PortalID)

            Return tabID
        End Function

        Public Sub ClearCache(ByVal portalId As Integer)
            DataCache.ClearTabsCache(portalId)

            'Clear the Portal cache so the Pages count is correct
            DataCache.ClearPortalCache(portalId, False)
        End Sub

        ''' -----------------------------------------------------------------------------
        ''' <summary>
        ''' Copies the modules from one tab to another
        ''' </summary>
        ''' <param name="PortalId">The Id iof the portal</param>
        ''' <param name="FromTabId">The Id of the tab to copy modules from.</param>
        ''' <param name="ToTabId">The Id of the tab to copy modules to.</param>
        ''' <param name="asReference">A flag indicating whether the module should be copied as a reference</param>
        ''' <history>
        ''' 	[cnurse]	04/30/2008	Created
        ''' </history>
        ''' -----------------------------------------------------------------------------
        Public Sub CopyTab(ByVal PortalId As Integer, ByVal FromTabId As Integer, ByVal ToTabId As Integer, ByVal asReference As Boolean)
            Dim objModules As New ModuleController
            Dim objModule As ModuleInfo

            For Each kvp As KeyValuePair(Of Integer, ModuleInfo) In objModules.GetTabModules(FromTabId)
                objModule = kvp.Value

                ' if the module shows on all pages does not need to be copied since it will
                ' be already added to this page
                If Not objModule.AllTabs Then
                    If asReference = False Then
                        objModule.ModuleID = Null.NullInteger
                    End If

                    objModule.TabID = ToTabId
                    objModules.AddModule(objModule)
                End If
            Next
        End Sub

        ''' -----------------------------------------------------------------------------
        ''' <summary>
        ''' Deletes a tab premanently from the database
        ''' </summary>
        ''' <param name="TabId">TabId of the tab to be deleted</param>
        ''' <param name="PortalId">PortalId of the portal</param>
        ''' <remarks>
        ''' </remarks>
        ''' <history>
        ''' 	[Vicen�]	19/09/2004	Added skin deassignment before deleting the tab.
        ''' </history>
        ''' -----------------------------------------------------------------------------
        Public Sub DeleteTab(ByVal TabId As Integer, ByVal PortalId As Integer)
            ' parent tabs can not be deleted
            If GetTabsByPortal(PortalId).WithParentId(TabId).Count = 0 Then
                'Fetch Tab
                Dim objTab As TabInfo = GetTab(TabId, PortalId, False)

                'Get the List of tabs with the same parent
                Dim siblingTabs As List(Of TabInfo) = GetTabsByPortal(PortalId).WithParentId(objTab.ParentId)
                Dim siblingCount As Integer = siblingTabs.Count

                'Find tab to delete
                For index As Integer = 0 To siblingCount - 1
                    Dim sibling As TabInfo = siblingTabs(index)
                    If sibling.TabID = TabId Then
                        DeleteTabInternal(TabId, PortalId)

                        'We need to update the taborder for the remaining items
                        UpdateTabOrder(siblingTabs, index + 1, siblingCount - 1, -2)
                        Exit For
                    End If
                Next

                ClearCache(PortalId)
                DataCache.RemoveCache(DataCache.PortalDictionaryCacheKey)
            End If
        End Sub

        ''' -----------------------------------------------------------------------------
        ''' <summary>
        ''' Deletes a tab premanently from the database
        ''' </summary>
        ''' <param name="TabId">TabId of the tab to be deleted</param>
        ''' <param name="PortalId">PortalId of the portal</param>
        ''' <remarks>
        ''' </remarks>
        ''' <history>
        ''' 	[Vicen�]	19/09/2004	Added skin deassignment before deleting the tab.
        ''' </history>
        ''' -----------------------------------------------------------------------------
        Public Sub DeleteTab(ByVal TabId As Integer, ByVal PortalId As Integer, ByVal deleteDescendants As Boolean)
            Dim descendantList As List(Of TabInfo) = GetTabsByPortal(PortalId).DescendentsOf(TabId)

            If deleteDescendants AndAlso descendantList.Count > 0 Then
                'Iterate through descendants from bottom - which will remove children first
                For i As Integer = descendantList.Count - 1 To 0 Step -1
                    DeleteTabInternal(descendantList(i).TabID, PortalId)
                Next
                ClearCache(PortalId)
            End If

            DeleteTab(TabId, PortalId)
        End Sub

        Public Function GetAllTabs(ByVal CheckLegacyFields As Boolean) As ArrayList
            Return CBO.FillCollection(provider.GetAllTabs(), GetType(TabInfo))
        End Function

        Public Function GetAllTabs() As ArrayList
            Return GetAllTabs(True)
        End Function

        Public Function GetTab(ByVal TabId As Integer, ByVal PortalId As Integer, ByVal ignoreCache As Boolean) As TabInfo
            Dim tab As TabInfo = Nothing
            Dim bFound As Boolean = False

            ' if we are using the cache
            If Not ignoreCache Then
                ' if we do not know the PortalId then try to find it in the Portals Dictionary using the TabId
                If Null.IsNull(PortalId) Then
                    Dim portalDic As Dictionary(Of Integer, Integer) = PortalController.GetPortalDictionary()
                    If portalDic IsNot Nothing AndAlso portalDic.ContainsKey(TabId) Then
                        PortalId = portalDic(TabId)
                    End If
                End If
                ' if we have the PortalId then try to get the TabInfo object from the Tabs Dictionary
                If Not Null.IsNull(PortalId) Then
                    Dim dicTabs As Dictionary(Of Integer, TabInfo)
                    dicTabs = GetTabsByPortal(PortalId)
                    bFound = dicTabs.TryGetValue(TabId, tab)
                End If
            End If

            ' if we are not using the cache or did not find the TabInfo object in the cache
            If ignoreCache Or Not bFound Then
                tab = CBO.FillObject(Of TabInfo)(provider.GetTab(TabId))
            End If

            Return tab
        End Function

        Public Function GetTabByName(ByVal TabName As String, ByVal PortalId As Integer) As TabInfo
            Return GetTabByNameAndParent(TabName, PortalId, Integer.MinValue)
        End Function

        Public Function GetTabByName(ByVal TabName As String, ByVal PortalId As Integer, ByVal ParentId As Integer) As TabInfo
            Return GetTabByNameAndParent(TabName, PortalId, ParentId)
        End Function

        Public Function GetTabCount(ByVal portalId As Integer) As Integer
            Return GetTabsByPortal(portalId).Count()
        End Function

        Public Function GetTabsByPortal(ByVal portalId As Integer) As TabCollection
            Dim cacheKey As String = String.Format(DataCache.TabCacheKey, portalId.ToString())
            Return CBO.GetCachedObject(Of TabCollection)(New CacheItemArgs(cacheKey, DataCache.TabCacheTimeOut, DataCache.TabCachePriority, portalId), _
                                                                                     AddressOf GetTabsByPortalCallBack)
        End Function

        Public Function GetTabsByModuleID(ByVal moduleID As Integer) As IDictionary(Of Integer, TabInfo)
            Return CBO.FillDictionary(Of Integer, TabInfo)("TabID", provider.GetTabsByModuleID(moduleID))
        End Function

        Public Function GetTabsByPackageID(ByVal portalID As Integer, ByVal packageID As Integer, ByVal forHost As Boolean) As IDictionary(Of Integer, TabInfo)
            Return CBO.FillDictionary(Of Integer, TabInfo)("TabID", provider.GetTabsByPackageID(portalID, packageID, forHost))
        End Function

        Public Sub MoveTab(ByVal objTab As TabInfo, ByVal type As TabMoveType)
            'Get the List of tabs with the same parent
            Dim siblingTabs As List(Of TabInfo) = GetTabsByPortal(objTab.PortalID).WithParentId(objTab.ParentId)
            Dim siblingCount As Integer = siblingTabs.Count

            'First make sure the list is sorted and spaced
            UpdateTabOrder(siblingTabs, 2)

            Select Case type
                Case TabMoveType.Top
                    'Tab is being moved to the top of the current level - Set TabOrder = 1
                    objTab.TabOrder = 1

                    'UpdateOrder - Parent hasn't changed so we don't need to regenerate TabPath
                    UpdateTabOrder(objTab, False)

                    'Get Tabs Index position in the Sibling List
                    Dim tabIndex As Integer = GetIndexOfTab(objTab, siblingTabs)

                    'We need to update the taborder for items that were before this tab
                    UpdateTabOrder(siblingTabs, 0, tabIndex - 1, 2)
                Case TabMoveType.Bottom    'Tab is being moved to the bottom of the current level
                    'Tab is being moved to the bottom of the current level - Set TabOrder = 2*TabCount - 1
                    objTab.TabOrder = siblingCount * 2 - 1

                    'UpdateOrder - Parent hasn't changed so we don't need to regenerate TabPath
                    UpdateTabOrder(objTab, False)

                    'Get Tabs Index position in the Sibling List
                    Dim tabIndex As Integer = GetIndexOfTab(objTab, siblingTabs)

                    'We need to update the taborder for items that were after this tab
                    UpdateTabOrder(siblingTabs, tabIndex + 1, siblingCount - 1, -2)
                Case TabMoveType.Up 'Tab is being moved up one position in the current level
                    'Get Tabs Index position in the Sibling List
                    Dim tabIndex As Integer = GetIndexOfTab(objTab, siblingTabs)

                    If tabIndex > 0 Then
                        'Get Tab that is one position up in the level
                        Dim swapTab As TabInfo = siblingTabs(tabIndex - 1)

                        'Swap the tab orders
                        SwapAdjacentTabs(objTab, swapTab)
                    End If
                Case TabMoveType.Down 'Tab is being moved down one position in the current level
                    'Get Tabs Index position in the Sibling List
                    Dim tabIndex As Integer = GetIndexOfTab(objTab, siblingTabs)

                    If tabIndex < siblingCount - 1 Then
                        'Get Tab that is one position down in the level
                        Dim swapTab As TabInfo = siblingTabs(tabIndex + 1)

                        'Swap the tab orders
                        SwapAdjacentTabs(swapTab, objTab)
                    End If
                Case TabMoveType.Promote 'Tab is being promoted to the next level up in the heirarchy
                    PromoteTab(objTab, siblingTabs)
                Case TabMoveType.Demote 'Tab is being demoted to the next level down in the heirarchy
                    DemoteTab(objTab, siblingTabs)
            End Select

            'Clear Cache
            ClearCache(objTab.PortalID)
        End Sub

        Public Sub UpdateTab(ByVal updatedTab As TabInfo)
            UpdateTab(updatedTab, Entities.Host.Host.ContentLocale.ToString)
        End Sub

        Public Sub UpdateTab(ByVal updatedTab As TabInfo, ByVal CultureCode As String)
            Dim originalTab As TabInfo = GetTab(updatedTab.TabID, updatedTab.PortalID, False)
            Dim updateOrder As Boolean = (originalTab.ParentId <> updatedTab.ParentId)
            Dim levelDelta As Integer = (updatedTab.Level - originalTab.Level)
            Dim updateChildren As Boolean = (originalTab.TabName <> updatedTab.TabName OrElse updateOrder)

            'Update Tab to DataStore
            provider.UpdateTab(updatedTab.TabID, updatedTab.ContentItemId, updatedTab.PortalID, updatedTab.TabName, updatedTab.IsVisible, updatedTab.DisableLink, _
                updatedTab.ParentId, updatedTab.IconFile, updatedTab.IconFileLarge, updatedTab.Title, updatedTab.Description, updatedTab.KeyWords, _
                updatedTab.IsDeleted, updatedTab.Url, updatedTab.SkinSrc, updatedTab.ContainerSrc, updatedTab.TabPath, _
                updatedTab.StartDate, updatedTab.EndDate, updatedTab.RefreshInterval, updatedTab.PageHeadText, _
                updatedTab.IsSecure, updatedTab.PermanentRedirect, updatedTab.SiteMapPriority, _
                UserController.GetCurrentUserInfo.UserID, CultureCode)

            'Update Tags
            Dim termController As ITermController = DotNetNuke.Entities.Content.Common.GetTermController()
            termController.RemoveTermsFromContent(updatedTab)
            For Each _Term As Term In updatedTab.Terms
                termController.AddTermToContent(_Term, updatedTab)
            Next

            Dim objEventLog As New Services.Log.EventLog.EventLogController
            objEventLog.AddLog(updatedTab, PortalController.GetCurrentPortalSettings, UserController.GetCurrentUserInfo.UserID, "", Services.Log.EventLog.EventLogController.EventLogType.TAB_UPDATED)

            'Update Tab permissions
            TabPermissionController.SaveTabPermissions(updatedTab)

            'Update TabSettings - use Try/catch as tabs are added during upgrade ptocess and the sproc may not exist
            Try
                UpdateTabSettings(updatedTab)
            Catch ex As Exception
                LogException(ex)
            End Try

            'Updated Tab Level
            If levelDelta <> 0 Then
                'Get the descendents
                Dim descendantTabs As List(Of TabInfo) = GetTabsByPortal(updatedTab.PortalID).DescendentsOf(updatedTab.TabID)

                'Update the Descendents of this tab
                UpdateDescendantLevel(descendantTabs, levelDelta)
            End If

            'Update Tab Order
            If updateOrder Then
                'Tab is being moved from the original list of siblings, so update the Taborder for the remaining tabs
                RemoveTab(updatedTab)

                'UpdateOrder - Parent has changed so we need to regenerate TabPath
                AddTabToEndOfList(updatedTab, True)
            End If

            'Update Tab Path for descendents
            If updateChildren Then
                'Clear Tab Cache to ensure that previous updates are picked up
                ClearCache(updatedTab.PortalID)
                UpdateChildTabPath(updatedTab.TabID, updatedTab.PortalID)
            End If

            'Clear Tab Caches
            ClearCache(updatedTab.PortalID)
            If updatedTab.PortalID <> originalTab.PortalID Then
                ClearCache(originalTab.PortalID)
            End If
        End Sub

        Private Sub UpdateTabSettings(ByRef updatedTab As TabInfo)
            Dim sKey As String
            For Each sKey In updatedTab.TabSettings.Keys
                UpdateTabSetting(updatedTab.TabID, sKey, CType(updatedTab.TabSettings(sKey), String))
            Next
        End Sub

        Public Sub UpdateTabOrder(ByVal objTab As TabInfo)
            UpdateTabOrder(objTab, True)
        End Sub

        Public Sub PopulateBreadCrumbs(ByRef tab As TabInfo)
            If (tab.BreadCrumbs Is Nothing) Then
                tab.BreadCrumbs = New ArrayList()
                PopulateBreadCrumbs(tab.PortalID, tab.BreadCrumbs, tab.TabID)
            End If
        End Sub

        Public Sub PopulateBreadCrumbs(ByVal portalID As Integer, ByRef breadCrumbs As ArrayList, ByVal tabID As Integer)
            ' find the tab in the tabs collection
            Dim objTab As TabInfo = Nothing
            Dim objTabController As New TabController()
            Dim portalTabs As TabCollection = objTabController.GetTabsByPortal(portalID)
            Dim hostTabs As TabCollection = objTabController.GetTabsByPortal(Null.NullInteger)

            Dim blnFound As Boolean = portalTabs.TryGetValue(tabID, objTab)
            If Not blnFound Then
                blnFound = hostTabs.TryGetValue(tabID, objTab)
            End If

            ' if tab was found
            If blnFound Then
                ' add tab to breadcrumb collection
                breadCrumbs.Insert(0, objTab.Clone)

                ' get the tab parent
                If Not Null.IsNull(objTab.ParentId) Then
                    PopulateBreadCrumbs(portalID, breadCrumbs, objTab.ParentId)
                End If
            End If
        End Sub

#Region "TabSettings"
        ''' <summary>
        ''' read all settings for a tab from TabSettings table
        ''' </summary>
        ''' <param name="TabId">ID of the Tab to query</param>
        ''' <returns>(cached) hashtable containing all settings</returns>
        ''' <history>
        '''    [jlucarino] 2009-08-31 Created
        ''' </history>
        Public Function GetTabSettings(ByVal TabId As Integer) As Hashtable
            Dim objSettings As Hashtable
            Dim strCacheKey As String = "GetTabSettings" & TabId.ToString

            objSettings = CType(DataCache.GetCache(strCacheKey), Hashtable)

            If objSettings Is Nothing Then
                objSettings = New Hashtable

                Dim dr As IDataReader = provider.GetTabSettings(TabId)
                While dr.Read()
                    If Not dr.IsDBNull(1) Then
                        objSettings(dr.GetString(0)) = dr.GetString(1)
                    Else
                        objSettings(dr.GetString(0)) = ""
                    End If
                End While

                dr.Close()

                ' cache data
                Dim intCacheTimeout As Integer = 20 * Convert.ToInt32(Host.Host.PerformanceSetting)
                DataCache.SetCache(strCacheKey, objSettings, TimeSpan.FromMinutes(intCacheTimeout))
            End If

            Return objSettings
        End Function

        ''' <summary>
        ''' Adds or updates a tab's setting value
        ''' </summary>
        ''' <param name="TabId">ID of the tab to update</param>
        ''' <param name="SettingName">name of the setting property</param>
        ''' <param name="SettingValue">value of the setting (String).</param>
        ''' <remarks>empty SettingValue will remove the setting, if not preserveIfEmpty is true</remarks>
        ''' <history>
        '''    [jlucarino] 2009-10-01 Created
        ''' </history>
        Public Sub UpdateTabSetting(ByVal TabId As Integer, ByVal SettingName As String, ByVal SettingValue As String)
            Dim objEventLog As New Services.Log.EventLog.EventLogController
            Dim objEventLogInfo As New Services.Log.EventLog.LogInfo
            objEventLogInfo.LogProperties.Add(New DotNetNuke.Services.Log.EventLog.LogDetailInfo("TabId", TabId.ToString))
            objEventLogInfo.LogProperties.Add(New DotNetNuke.Services.Log.EventLog.LogDetailInfo("SettingName", SettingName.ToString))
            objEventLogInfo.LogProperties.Add(New DotNetNuke.Services.Log.EventLog.LogDetailInfo("SettingValue", SettingValue.ToString))

            Dim dr As IDataReader = provider.GetTabSetting(TabId, SettingName)

            If dr.Read Then
                provider.UpdateTabSetting(TabId, SettingName, SettingValue, UserController.GetCurrentUserInfo.UserID)
                objEventLogInfo.LogTypeKey = Log.EventLog.EventLogController.EventLogType.TAB_SETTING_UPDATED.ToString
                objEventLog.AddLog(objEventLogInfo)
            Else
                provider.AddTabSetting(TabId, SettingName, SettingValue, UserController.GetCurrentUserInfo.UserID)
                objEventLogInfo.LogTypeKey = Log.EventLog.EventLogController.EventLogType.TAB_SETTING_CREATED.ToString
                objEventLog.AddLog(objEventLogInfo)
            End If
            dr.Close()

            DataCache.RemoveCache("GetTabSettings" & TabId.ToString)

        End Sub

        ''' <summary>
        ''' Delete a Setting of a tab instance
        ''' </summary>
        ''' <param name="TabId">ID of the affected tab</param>
        ''' <param name="SettingName">Name of the setting to be deleted</param>
        ''' <history>
        '''    [jlucarino] 2009-10-01 Created
        ''' </history>
        Public Sub DeleteTabSetting(ByVal TabId As Integer, ByVal SettingName As String)
            provider.DeleteTabSetting(TabId, SettingName)
            Dim objEventLog As New Services.Log.EventLog.EventLogController
            Dim objEventLogInfo As New Services.Log.EventLog.LogInfo
            objEventLogInfo.LogProperties.Add(New DotNetNuke.Services.Log.EventLog.LogDetailInfo("TabID", TabId.ToString))
            objEventLogInfo.LogProperties.Add(New DotNetNuke.Services.Log.EventLog.LogDetailInfo("SettingName", SettingName.ToString))
            objEventLogInfo.LogTypeKey = Log.EventLog.EventLogController.EventLogType.TAB_SETTING_DELETED.ToString
            objEventLog.AddLog(objEventLogInfo)
            DataCache.RemoveCache("GetTabSettings" & TabId.ToString)
        End Sub

        ''' <summary>
        ''' Delete all Settings of a tab instance
        ''' </summary>
        ''' <param name="TabId">ID of the affected tab</param>
        ''' <history>
        '''    [jlucarino] 2009-10-01 Created
        ''' </history>
        Public Sub DeleteTabSettings(ByVal TabId As Integer)
            provider.DeleteTabSettings(TabId)
            Dim objEventLog As New Services.Log.EventLog.EventLogController
            Dim objEventLogInfo As New Services.Log.EventLog.LogInfo
            objEventLogInfo.LogProperties.Add(New DotNetNuke.Services.Log.EventLog.LogDetailInfo("TabId", TabId.ToString))
            objEventLogInfo.LogTypeKey = Log.EventLog.EventLogController.EventLogType.TAB_SETTING_DELETED.ToString
            objEventLog.AddLog(objEventLogInfo)
            DataCache.RemoveCache("GetTabSettings" & TabId.ToString)
        End Sub
#End Region

#End Region

#Region "Private Shared Methods"

        Private Shared Function DeleteChildTabs(ByVal intTabid As Integer, ByVal PortalSettings As PortalSettings, ByVal UserId As Integer) As Boolean
            Dim objtabs As New TabController
            Dim bDeleted As Boolean = True

            For Each objtab As TabInfo In GetTabsByParent(intTabid, PortalSettings.PortalId)
                bDeleted = DeleteTab(objtab, PortalSettings, UserId)
                If Not bDeleted Then
                    Exit For
                End If
            Next

            Return bDeleted
        End Function

        Private Shared Function DeleteTab(ByVal objtab As TabInfo, ByVal PortalSettings As PortalSettings, ByVal UserId As Integer) As Boolean
            Dim objtabs As New TabController
            Dim bDeleted As Boolean = True

            If Not IsSpecialTab(objtab.TabID, PortalSettings) Then
                'delete child tabs
                If DeleteChildTabs(objtab.TabID, PortalSettings, UserId) Then
                    objtab.IsDeleted = True
                    objtabs.UpdateTab(objtab)

                    Dim objEventLog As New Services.Log.EventLog.EventLogController
                    objEventLog.AddLog(objtab, PortalSettings, UserId, "", Services.Log.EventLog.EventLogController.EventLogType.TAB_SENT_TO_RECYCLE_BIN)
                Else
                    bDeleted = False
                End If
            Else
                bDeleted = False
            End If

            Return bDeleted

        End Function

        Private Shared Sub DeserializeTabSettings(ByVal nodeTabSettings As XmlNodeList, ByVal objTab As TabInfo)
            Dim oTabSettingNode As XmlNode
            Dim sKey As String
            Dim sValue As String

            For Each oTabSettingNode In nodeTabSettings
                sKey = XmlUtils.GetNodeValue(oTabSettingNode, "settingname")
                sValue = XmlUtils.GetNodeValue(oTabSettingNode, "settingvalue")

                objTab.TabSettings(sKey) = sValue
            Next
        End Sub

        ''' -----------------------------------------------------------------------------
        ''' <summary>
        ''' Deserializes tab permissions
        ''' </summary>
        ''' <param name="nodeTabPermissions">Node for tab permissions</param>
        ''' <param name="objTab">Tab being processed</param>
        ''' <param name="IsAdminTemplate">Flag to indicate if we are parsing admin template</param>
        ''' <remarks>
        ''' </remarks>
        ''' <history>
        ''' 	[Vicen�]	15/10/2004	Created
        '''     [cnurse]    10/02/2007  Moved from PortalController
        ''' </history>
        ''' -----------------------------------------------------------------------------
        Private Shared Sub DeserializeTabPermissions(ByVal nodeTabPermissions As XmlNodeList, ByVal objTab As TabInfo, ByVal IsAdminTemplate As Boolean)
            Dim objPermissionController As New Security.Permissions.PermissionController
            Dim objPermission As Security.Permissions.PermissionInfo
            Dim objTabPermission As Security.Permissions.TabPermissionInfo
            Dim objRoleController As New RoleController
            Dim objRole As RoleInfo
            Dim RoleID As Integer
            Dim PermissionID As Integer
            Dim PermissionKey, PermissionCode As String
            Dim RoleName As String
            Dim AllowAccess As Boolean
            Dim arrPermissions As ArrayList
            Dim i As Integer
            Dim xmlTabPermission As XmlNode

            For Each xmlTabPermission In nodeTabPermissions
                PermissionKey = XmlUtils.GetNodeValue(xmlTabPermission, "permissionkey")
                PermissionCode = XmlUtils.GetNodeValue(xmlTabPermission, "permissioncode")
                RoleName = XmlUtils.GetNodeValue(xmlTabPermission, "rolename")
                AllowAccess = XmlUtils.GetNodeValueBoolean(xmlTabPermission, "allowaccess")
                arrPermissions = objPermissionController.GetPermissionByCodeAndKey(PermissionCode, PermissionKey)

                For i = 0 To arrPermissions.Count - 1
                    objPermission = CType(arrPermissions(i), Security.Permissions.PermissionInfo)
                    PermissionID = objPermission.PermissionID
                Next
                RoleID = Integer.MinValue
                Select Case RoleName
                    Case glbRoleAllUsersName
                        RoleID = Convert.ToInt32(glbRoleAllUsers)
                    Case Common.Globals.glbRoleUnauthUserName
                        RoleID = Convert.ToInt32(glbRoleUnauthUser)
                    Case Else
                        Dim objPortals As New PortalController
                        Dim objPortal As PortalInfo = objPortals.GetPortal(objTab.PortalID)
                        objRole = objRoleController.GetRoleByName(objPortal.PortalID, RoleName)
                        If Not objRole Is Nothing Then
                            RoleID = objRole.RoleID
                        Else
                            ' if parsing admin.template and role administrators redefined, use portal.administratorroleid
                            If IsAdminTemplate AndAlso RoleName.ToLower() = "administrators" Then
                                RoleID = objPortal.AdministratorRoleId
                            End If
                        End If
                End Select

                ' if role was found add, otherwise ignore
                If RoleID <> Integer.MinValue Then
                    objTabPermission = New Security.Permissions.TabPermissionInfo
                    objTabPermission.TabID = objTab.TabID
                    objTabPermission.PermissionID = PermissionID
                    objTabPermission.RoleID = RoleID
                    objTabPermission.AllowAccess = AllowAccess
                    objTab.TabPermissions.Add(objTabPermission)
                End If
            Next
        End Sub

        ''' -----------------------------------------------------------------------------
        ''' <summary>
        ''' GetTabsByPortalCallBack gets a Dictionary of Tabs by 
        ''' Portal from the the Database.
        ''' </summary>
        ''' <param name="cacheItemArgs">The CacheItemArgs object that contains the parameters
        ''' needed for the database call</param>
        ''' <history>
        ''' 	[cnurse]	01/15/2008   Created
        ''' </history>
        ''' -----------------------------------------------------------------------------
        Private Shared Function GetTabsByPortalCallBack(ByVal cacheItemArgs As CacheItemArgs) As Object
            Dim portalID As Integer = DirectCast(cacheItemArgs.ParamList(0), Integer)
            Dim tabs As List(Of TabInfo) = CBO.FillCollection(Of TabInfo)(provider.GetTabs(portalID))
            Return New TabCollection(tabs)
        End Function

        Private Shared Function GetTabPathDictionaryCallback(ByVal cacheItemArgs As CacheItemArgs) As Object
            Dim portalID As Integer = DirectCast(cacheItemArgs.ParamList(0), Integer)
            Dim tabpathDic As New Dictionary(Of String, Integer)(StringComparer.CurrentCultureIgnoreCase)
            Dim dr As IDataReader = DataProvider.Instance().GetTabPaths(portalID)
            Try
                While dr.Read
                    ' add to dictionary
                    tabpathDic(Null.SetNullString(dr("TabPath"))) = Null.SetNullInteger(dr("TabID"))
                End While
            Catch exc As Exception
                LogException(exc)
            Finally
                ' close datareader
                CBO.CloseDataReader(dr, True)
            End Try

            Return tabpathDic
        End Function

#End Region

#Region "Public Shared Methods"

        Public Shared Sub CopyDesignToChildren(ByVal parentTab As TabInfo, ByVal skinSrc As String, ByVal containerSrc As String)
            CopyDesignToChildren(parentTab, skinSrc, containerSrc, Entities.Host.Host.ContentLocale.ToString)
        End Sub

        Public Shared Sub CopyDesignToChildren(ByVal parentTab As TabInfo, ByVal skinSrc As String, ByVal containerSrc As String, ByVal CultureCode As String)
            Dim clearCache As Boolean = Null.NullBoolean
            Dim childTabs As List(Of TabInfo) = New TabController().GetTabsByPortal(parentTab.PortalID).DescendentsOf(parentTab.TabID)

            For Each objTab As TabInfo In childTabs
                If TabPermissionController.CanAdminPage(objTab) Then
                    provider.UpdateTab(objTab.TabID, objTab.ContentItemId, objTab.PortalID, objTab.TabName, objTab.IsVisible, objTab.DisableLink, _
                          objTab.ParentId, objTab.IconFile, objTab.IconFileLarge, objTab.Title, objTab.Description, objTab.KeyWords, _
                          objTab.IsDeleted, objTab.Url, skinSrc, containerSrc, objTab.TabPath, objTab.StartDate, _
                          objTab.EndDate, objTab.RefreshInterval, objTab.PageHeadText, objTab.IsSecure, objTab.PermanentRedirect, _
                          objTab.SiteMapPriority, UserController.GetCurrentUserInfo.UserID, CultureCode)

                    Dim objEventLog As New Services.Log.EventLog.EventLogController
                    objEventLog.AddLog(objTab, PortalController.GetCurrentPortalSettings, UserController.GetCurrentUserInfo.UserID, "", Services.Log.EventLog.EventLogController.EventLogType.TAB_UPDATED)
                    clearCache = True
                End If
            Next

            If clearCache Then
                DotNetNuke.Common.Utilities.DataCache.ClearTabsCache(childTabs(0).PortalID)
            End If
        End Sub

        Public Shared Sub CopyPermissionsToChildren(ByVal parentTab As TabInfo, ByVal newPermissions As Permissions.TabPermissionCollection)
            Dim objTabPermissionController As New Security.Permissions.TabPermissionController
            Dim clearCache As Boolean = Null.NullBoolean

            Dim childTabs As List(Of TabInfo) = New TabController().GetTabsByPortal(parentTab.PortalID).DescendentsOf(parentTab.TabID)

            For Each objTab As TabInfo In childTabs
                If TabPermissionController.CanAdminPage(objTab) Then
                    objTab.TabPermissions.Clear()
                    objTab.TabPermissions.AddRange(newPermissions)

                    TabPermissionController.SaveTabPermissions(objTab)
                    clearCache = True
                End If
            Next

            If clearCache Then
                DotNetNuke.Common.Utilities.DataCache.ClearTabsCache(childTabs(0).PortalID)
            End If
        End Sub

        Public Shared Function DeleteTab(ByVal tabId As Integer, ByVal PortalSettings As PortalSettings, ByVal UserId As Integer) As Boolean
            Dim bDeleted As Boolean = True
            Dim objController As New TabController()

            Dim objTab As TabInfo = objController.GetTab(tabId, PortalSettings.PortalId, False)
            If objTab IsNot Nothing Then
                'Get the List of tabs with the same parent
                Dim siblingTabs As List(Of TabInfo) = objController.GetTabsByPortal(objTab.PortalID).WithParentId(objTab.ParentId)
                Dim siblingCount As Integer = siblingTabs.Count

                'First make sure the list is sorted and spaced
                objController.UpdateTabOrder(siblingTabs, 2)

                'Get Tabs Index position in the Sibling List
                Dim tabIndex As Integer = objController.GetIndexOfTab(objTab, siblingTabs)
                bDeleted = DeleteTab(objTab, PortalSettings, UserId)

                'Clear deleted tabs TabOrder
                objTab.TabOrder = 0

                'Update TabOrder
                objController.UpdateTabOrder(objTab, False)

                'We need to update the taborder for items that were after this tab
                objController.UpdateTabOrder(siblingTabs, tabIndex + 1, siblingCount - 1, -2)
            Else
                bDeleted = False
            End If

            Return bDeleted
        End Function

        ''' -----------------------------------------------------------------------------
        ''' <summary>
        ''' Processes all panes and modules in the template file
        ''' </summary>
        ''' <param name="nodePanes">Template file node for the panes is current tab</param>
        ''' <param name="PortalId">PortalId of the new portal</param>
        ''' <param name="TabId">Tab being processed</param>
        ''' <remarks>
        ''' </remarks>
        ''' <history>
        ''' 	[VMasanas]	03/09/2004	Created
        ''' 	[VMasanas]	15/10/2004	Modified for new skin structure
        '''		[cnurse]	15/10/2004	Modified to allow for merging template
        '''								with existing pages
        '''     [cnurse]    10/02/2007  Moved from PortalController
        ''' </history>
        ''' -----------------------------------------------------------------------------
        Public Shared Sub DeserializePanes(ByVal nodePanes As XmlNode, ByVal PortalId As Integer, ByVal TabId As Integer, ByVal mergeTabs As PortalTemplateModuleAction, ByVal hModules As Hashtable)
            Dim objModules As New ModuleController
            Dim objModule As ModuleInfo

            Dim dicModules As Dictionary(Of Integer, ModuleInfo) = objModules.GetTabModules(TabId)

            'If Mode is Replace remove all the modules already on this Tab
            If mergeTabs = PortalTemplateModuleAction.Replace Then
                For Each kvp As KeyValuePair(Of Integer, ModuleInfo) In dicModules
                    objModule = kvp.Value
                    objModules.DeleteTabModule(TabId, objModule.ModuleID, False)
                Next
            End If

            ' iterate through the panes
            For Each nodePane As XmlNode In nodePanes.ChildNodes
                ' iterate through the modules
                If Not nodePane.SelectSingleNode("modules") Is Nothing Then
                    For Each nodeModule As XmlNode In nodePane.SelectSingleNode("modules")
                        ModuleController.DeserializeModule(nodeModule, nodePane, PortalId, TabId, mergeTabs, hModules)
                    Next
                End If
            Next
        End Sub

        Public Shared Function DeserializeTab(ByVal nodeTab As XmlNode, ByVal objTab As TabInfo, ByVal PortalId As Integer, ByVal mergeTabs As PortalTemplateModuleAction) As TabInfo
            Return TabController.DeserializeTab(nodeTab, objTab, New Hashtable(), PortalId, False, mergeTabs, New Hashtable())
        End Function

        Public Shared Function DeserializeTab(ByVal nodeTab As XmlNode, ByVal objTab As TabInfo, ByVal hTabs As Hashtable, ByVal PortalId As Integer, ByVal IsAdminTemplate As Boolean, ByVal mergeTabs As PortalTemplateModuleAction, ByVal hModules As Hashtable) As TabInfo
            Dim objTabs As New TabController
            Dim tabName As String = XmlUtils.GetNodeValue(nodeTab, "name")

            If tabName <> "" Then
                If objTab Is Nothing Then
                    objTab = New TabInfo
                    objTab.TabID = Null.NullInteger
                    objTab.ParentId = Null.NullInteger
                    objTab.TabName = tabName
                End If
                objTab.PortalID = PortalId
                objTab.Title = XmlUtils.GetNodeValue(nodeTab, "title")
                objTab.Description = XmlUtils.GetNodeValue(nodeTab, "description")
                objTab.KeyWords = XmlUtils.GetNodeValue(nodeTab, "keywords")
                objTab.IsVisible = XmlUtils.GetNodeValueBoolean(nodeTab, "visible", True)
                objTab.DisableLink = XmlUtils.GetNodeValueBoolean(nodeTab, "disabled")
                objTab.IconFile = ImportFile(PortalId, XmlUtils.GetNodeValue(nodeTab, "iconfile"))
                objTab.IconFileLarge = ImportFile(PortalId, XmlUtils.GetNodeValue(nodeTab, "iconfilelarge"))
                objTab.Url = XmlUtils.GetNodeValue(nodeTab, "url")
                objTab.StartDate = XmlUtils.GetNodeValueDate(nodeTab, "startdate", Null.NullDate)
                objTab.EndDate = XmlUtils.GetNodeValueDate(nodeTab, "enddate", Null.NullDate)
                objTab.RefreshInterval = XmlUtils.GetNodeValueInt(nodeTab, "refreshinterval", Null.NullInteger)
                objTab.PageHeadText = XmlUtils.GetNodeValue(nodeTab, "pageheadtext", Null.NullString)
                objTab.IsSecure = XmlUtils.GetNodeValueBoolean(nodeTab, "issecure", False)
                objTab.SiteMapPriority = XmlUtils.GetNodeValueSingle(nodeTab, "sitemappriority", 0.5)

                objTab.TabPermissions.Clear()
                DeserializeTabPermissions(nodeTab.SelectNodes("tabpermissions/permission"), objTab, IsAdminTemplate)

                DeserializeTabSettings(nodeTab.SelectNodes("tabsettings/tabsetting"), objTab)

                ' set tab skin and container
                If XmlUtils.GetNodeValue(nodeTab, "skinsrc", "") <> "" Then
                    objTab.SkinSrc = XmlUtils.GetNodeValue(nodeTab, "skinsrc", "")
                End If
                If XmlUtils.GetNodeValue(nodeTab, "containersrc", "") <> "" Then
                    objTab.ContainerSrc = XmlUtils.GetNodeValue(nodeTab, "containersrc", "")
                End If

                tabName = objTab.TabName
                If XmlUtils.GetNodeValue(nodeTab, "parent") <> "" Then
                    If Not hTabs(XmlUtils.GetNodeValue(nodeTab, "parent")) Is Nothing Then
                        ' parent node specifies the path (tab1/tab2/tab3), use saved tabid
                        objTab.ParentId = Convert.ToInt32(hTabs(XmlUtils.GetNodeValue(nodeTab, "parent")))
                        tabName = XmlUtils.GetNodeValue(nodeTab, "parent") + "/" + objTab.TabName
                    Else
                        ' Parent node doesn't spcecify the path, search by name.
                        ' Possible incoherence if tabname not unique
                        Dim objParent As TabInfo = objTabs.GetTabByName(XmlUtils.GetNodeValue(nodeTab, "parent"), PortalId)
                        If Not objParent Is Nothing Then
                            objTab.ParentId = objParent.TabID
                            tabName = objParent.TabName + "/" + objTab.TabName
                        Else
                            ' parent tab not found!
                            objTab.ParentId = Null.NullInteger
                            tabName = objTab.TabName
                        End If
                    End If
                End If

                ' create/update tab
                If objTab.TabID = Null.NullInteger Then
                    objTab.TabID = objTabs.AddTab(objTab)
                Else
                    objTabs.UpdateTab(objTab)
                End If

                ' extra check for duplicate tabs in same level
                If hTabs(tabName) Is Nothing Then
                    hTabs.Add(tabName, objTab.TabID)
                End If
            End If

            'Parse Panes
            If Not nodeTab.SelectSingleNode("panes") Is Nothing Then
                DeserializePanes(nodeTab.SelectSingleNode("panes"), PortalId, objTab.TabID, mergeTabs, hModules)
            End If

            'Finally add "tabid" to node
            nodeTab.AppendChild(XmlUtils.CreateElement(nodeTab.OwnerDocument, "tabid", objTab.TabID.ToString()))

            Return objTab
        End Function

        Public Shared Function GetPortalTabs(ByVal portalId As Integer, ByVal excludeTabId As Integer, ByVal includeNoneSpecified As Boolean, ByVal includeHidden As Boolean) As List(Of TabInfo)
            Return GetPortalTabs(GetTabsBySortOrder(portalId), excludeTabId, includeNoneSpecified, "<" + Localization.GetString("None_Specified") + ">", includeHidden, False, False, False, False)
        End Function

        Public Shared Function GetPortalTabs(ByVal portalId As Integer, ByVal excludeTabId As Integer, ByVal includeNoneSpecified As Boolean, ByVal includeHidden As Boolean, ByVal includeDeleted As Boolean, ByVal includeURL As Boolean) As List(Of TabInfo)
            Return GetPortalTabs(GetTabsBySortOrder(portalId), excludeTabId, includeNoneSpecified, "<" + Localization.GetString("None_Specified") + ">", includeHidden, includeDeleted, includeURL, False, False)
        End Function

        Public Shared Function GetPortalTabs(ByVal portalId As Integer, ByVal excludeTabId As Integer, ByVal includeNoneSpecified As Boolean, ByVal NoneSpecifiedText As String, ByVal includeHidden As Boolean, ByVal includeDeleted As Boolean, ByVal includeURL As Boolean, ByVal checkViewPermisison As Boolean, ByVal checkEditPermission As Boolean) As List(Of TabInfo)
            Return GetPortalTabs(GetTabsBySortOrder(portalId), excludeTabId, includeNoneSpecified, NoneSpecifiedText, includeHidden, includeDeleted, includeURL, checkViewPermisison, checkEditPermission)
        End Function

        Public Shared Function GetPortalTabs(ByVal tabs As List(Of TabInfo), ByVal excludeTabId As Integer, ByVal includeNoneSpecified As Boolean, ByVal NoneSpecifiedText As String, ByVal includeHidden As Boolean, ByVal includeDeleted As Boolean, ByVal includeURL As Boolean, ByVal checkViewPermisison As Boolean, ByVal checkEditPermission As Boolean) As List(Of TabInfo)
            Dim listTabs As New List(Of TabInfo)

            If includeNoneSpecified Then
                Dim objTab As New TabInfo
                objTab.TabID = -1
                objTab.TabName = NoneSpecifiedText
                objTab.TabOrder = 0
                objTab.ParentId = -2
                listTabs.Add(objTab)
            End If

            For Each objTab As TabInfo In tabs
                Dim objUserInfo As UserInfo = UserController.GetCurrentUserInfo
                If ((excludeTabId < 0) OrElse (objTab.TabID <> excludeTabId)) AndAlso (Not objTab.IsSuperTab OrElse objUserInfo.IsSuperUser) Then
                    If (objTab.IsVisible = True OrElse includeHidden = True) AndAlso _
                      (objTab.IsDeleted = False OrElse includeDeleted = True) AndAlso _
                      (objTab.TabType = TabType.Normal Or includeURL = True) Then

                        'Check if User has View/Edit Permission for this tab
                        If checkEditPermission OrElse checkViewPermisison Then
                            Dim permissionList As String = "ADD,COPY,EDIT,MANAGE"
                            If checkEditPermission AndAlso TabPermissionController.HasTabPermission(objTab.TabPermissions, permissionList) Then
                                listTabs.Add(objTab)
                            ElseIf checkViewPermisison AndAlso TabPermissionController.CanViewPage(objTab) Then
                                listTabs.Add(objTab)
                            End If
                        Else
                            'Add Tab to List
                            listTabs.Add(objTab)
                        End If
                    End If
                End If
            Next
            Return listTabs
        End Function

        Public Shared Function GetTabByTabPath(ByVal portalId As Integer, ByVal tabPath As String) As Integer
            Dim tabpathDic As Dictionary(Of String, Integer) = GetTabPathDictionary(portalId)
            If tabpathDic.ContainsKey(tabPath) Then
                Return tabpathDic(tabPath)
            Else
                Return -1
            End If
        End Function

        Public Shared Function GetTabPathDictionary(ByVal portalId As Integer) As Dictionary(Of String, Integer)
            Dim cacheKey As String = String.Format(DataCache.TabPathCacheKey, portalId.ToString())
            Return CBO.GetCachedObject(Of Dictionary(Of String, Integer))(New CacheItemArgs(cacheKey, DataCache.TabPathCacheTimeOut, _
              DataCache.TabPathCachePriority, portalId), AddressOf GetTabPathDictionaryCallback)
        End Function

        Public Shared Function GetTabsByParent(ByVal parentId As Integer, ByVal portalId As Integer) As List(Of TabInfo)
            Return New TabController().GetTabsByPortal(portalId).WithParentId(parentId)
        End Function

        Public Shared Function GetTabsBySortOrder(ByVal portalId As Integer) As List(Of TabInfo)
            Return New TabController().GetTabsByPortal(portalId).AsList()
        End Function

        Public Shared Function IsSpecialTab(ByVal tabId As Integer, ByVal PortalSettings As PortalSettings) As Boolean
            Return tabId = PortalSettings.SplashTabId OrElse tabId = PortalSettings.HomeTabId OrElse _
              tabId = PortalSettings.LoginTabId OrElse tabId = PortalSettings.UserTabId OrElse _
              tabId = PortalSettings.AdminTabId OrElse tabId = PortalSettings.SuperTabId
        End Function

        Public Shared Sub RestoreTab(ByVal objTab As TabInfo, ByVal PortalSettings As PortalSettings, ByVal UserId As Integer)
            Dim objEventLog As New Services.Log.EventLog.EventLogController
            Dim objController As New TabController

            objTab.IsDeleted = False
            objController.UpdateTab(objTab)

            'Get the List of tabs with the same parent
            Dim siblingTabs As List(Of TabInfo) = objController.GetTabsByPortal(objTab.PortalID).WithParentId(objTab.ParentId)
            Dim siblingCount As Integer = siblingTabs.Count

            objTab.TabOrder = 2 * siblingTabs.Count + 1

            'UpdateOrder 
            objController.UpdateTabOrder(objTab, False)

            objEventLog.AddLog(objTab, PortalSettings, UserId, "", Services.Log.EventLog.EventLogController.EventLogType.TAB_RESTORED)

            Dim objmodules As New ModuleController
            Dim arrMods As ArrayList = objmodules.GetAllTabsModules(objTab.PortalID, True)

            For Each objModule As ModuleInfo In arrMods
                objmodules.CopyModule(objModule.ModuleID, objModule.TabID, objTab.TabID, "", True)
            Next

            objController.ClearCache(objTab.PortalID)
        End Sub

        ''' <summary>
        ''' SerializeTab
        ''' </summary>
        ''' <param name="xmlTab">The Xml Document to use for the Tab</param>
        ''' <param name="objTab">The TabInfo object to serialize</param>
        ''' <param name="includeContent">A flag used to determine if the Module content is included</param>
        Public Shared Function SerializeTab(ByVal xmlTab As XmlDocument, ByVal objTab As TabInfo, ByVal includeContent As Boolean) As XmlNode
            Return SerializeTab(xmlTab, Nothing, objTab, Nothing, includeContent)
        End Function

        ''' <summary>
        ''' SerializeTab
        ''' </summary>
        ''' <param name="xmlTab">The Xml Document to use for the Tab</param>
        ''' <param name="hTabs">A Hashtable used to store the names of the tabs</param>
        ''' <param name="objTab">The TabInfo object to serialize</param>
        ''' <param name="objPortal">The Portal object to which the tab belongs</param>
        ''' <param name="includeContent">A flag used to determine if the Module content is included</param>
        Public Shared Function SerializeTab(ByVal xmlTab As XmlDocument, ByVal hTabs As Hashtable, ByVal objTab As TabInfo, ByVal objPortal As PortalInfo, ByVal includeContent As Boolean) As XmlNode
            Dim nodeTab, urlNode, newnode As XmlNode

            CBO.SerializeObject(objTab, xmlTab)

            nodeTab = xmlTab.SelectSingleNode("tab")
            nodeTab.Attributes.Remove(nodeTab.Attributes.ItemOf("xmlns:xsd"))
            nodeTab.Attributes.Remove(nodeTab.Attributes.ItemOf("xmlns:xsi"))

            'remove unwanted elements
            nodeTab.RemoveChild(nodeTab.SelectSingleNode("tabid"))
            nodeTab.RemoveChild(nodeTab.SelectSingleNode("taborder"))
            nodeTab.RemoveChild(nodeTab.SelectSingleNode("portalid"))
            nodeTab.RemoveChild(nodeTab.SelectSingleNode("parentid"))
            nodeTab.RemoveChild(nodeTab.SelectSingleNode("isdeleted"))
            nodeTab.RemoveChild(nodeTab.SelectSingleNode("tabpath"))
            nodeTab.RemoveChild(nodeTab.SelectSingleNode("haschildren"))
            nodeTab.RemoveChild(nodeTab.SelectSingleNode("skindoctype"))

            For Each nodePermission As XmlNode In nodeTab.SelectNodes("tabpermissions/permission")
                nodePermission.RemoveChild(nodePermission.SelectSingleNode("tabpermissionid"))
                nodePermission.RemoveChild(nodePermission.SelectSingleNode("permissionid"))
                nodePermission.RemoveChild(nodePermission.SelectSingleNode("tabid"))
                nodePermission.RemoveChild(nodePermission.SelectSingleNode("roleid"))
                nodePermission.RemoveChild(nodePermission.SelectSingleNode("userid"))
                nodePermission.RemoveChild(nodePermission.SelectSingleNode("username"))
                nodePermission.RemoveChild(nodePermission.SelectSingleNode("displayname"))
            Next

            'Manage Url
            urlNode = xmlTab.SelectSingleNode("tab/url")
            Select Case objTab.TabType
                Case TabType.Normal
                    urlNode.Attributes.Append(XmlUtils.CreateAttribute(xmlTab, "type", "Normal"))
                Case TabType.Tab
                    urlNode.Attributes.Append(XmlUtils.CreateAttribute(xmlTab, "type", "Tab"))
                    'Get the tab being linked to
                    Dim tab As TabInfo = New TabController().GetTab(Int32.Parse(objTab.Url), objTab.PortalID, False)
                    urlNode.InnerXml = tab.TabPath
                Case TabType.File
                    urlNode.Attributes.Append(XmlUtils.CreateAttribute(xmlTab, "type", "File"))
                    Dim file As Services.FileSystem.FileInfo = New Services.FileSystem.FileController().GetFileById(Int32.Parse(objTab.Url.Substring(7)), objTab.PortalID)
                    urlNode.InnerXml = file.RelativePath
                Case TabType.Url
                    urlNode.Attributes.Append(XmlUtils.CreateAttribute(xmlTab, "type", "Url"))
            End Select

            'serialize TabSettings
            XmlUtils.SerializeHashtable(objTab.TabSettings, xmlTab, nodeTab, "tabsetting", "settingname", "settingvalue")

            If objPortal IsNot Nothing Then
                Select Case objTab.TabID
                    Case objPortal.SplashTabId
                        newnode = xmlTab.CreateElement("tabtype")
                        newnode.InnerXml = "splashtab"
                        nodeTab.AppendChild(newnode)
                    Case objPortal.HomeTabId
                        newnode = xmlTab.CreateElement("tabtype")
                        newnode.InnerXml = "hometab"
                        nodeTab.AppendChild(newnode)
                    Case objPortal.UserTabId
                        newnode = xmlTab.CreateElement("tabtype")
                        newnode.InnerXml = "usertab"
                        nodeTab.AppendChild(newnode)
                    Case objPortal.LoginTabId
                        newnode = xmlTab.CreateElement("tabtype")
                        newnode.InnerXml = "logintab"
                        nodeTab.AppendChild(newnode)
                End Select
            End If

            'Manage Parent Tab
            If hTabs IsNot Nothing Then
                If Not Null.IsNull(objTab.ParentId) Then
                    newnode = xmlTab.CreateElement("parent")
                    newnode.InnerXml = HttpContext.Current.Server.HtmlEncode(hTabs(objTab.ParentId).ToString)
                    nodeTab.AppendChild(newnode)

                    ' save tab as: ParentTabName/CurrentTabName
                    hTabs.Add(objTab.TabID, hTabs(objTab.ParentId).ToString + "/" + objTab.TabName)
                Else
                    ' save tab as: CurrentTabName
                    hTabs.Add(objTab.TabID, objTab.TabName)
                End If
            End If

            Dim nodePanes, nodePane, nodeName, nodeModules, nodeModule As XmlNode
            Dim xmlModule As XmlDocument
            Dim objmodule As ModuleInfo
            Dim objmodules As New ModuleController

            ' Serialize modules
            nodePanes = nodeTab.AppendChild(xmlTab.CreateElement("panes"))

            For Each kvp As KeyValuePair(Of Integer, ModuleInfo) In objmodules.GetTabModules(objTab.TabID)
                objmodule = kvp.Value
                If Not objmodule.IsDeleted Then
                    xmlModule = New XmlDocument
                    nodeModule = ModuleController.SerializeModule(xmlModule, objmodule, includeContent)

                    If nodePanes.SelectSingleNode("descendant::pane[name='" & objmodule.PaneName & "']") Is Nothing Then
                        ' new pane found
                        nodePane = xmlModule.CreateElement("pane")
                        nodeName = nodePane.AppendChild(xmlModule.CreateElement("name"))
                        nodeName.InnerText = objmodule.PaneName
                        nodePane.AppendChild(xmlModule.CreateElement("modules"))
                        nodePanes.AppendChild(xmlTab.ImportNode(nodePane, True))
                    End If
                    nodeModules = nodePanes.SelectSingleNode("descendant::pane[name='" & objmodule.PaneName & "']/modules")

                    nodeModules.AppendChild(xmlTab.ImportNode(nodeModule, True))
                End If
            Next
            Return nodeTab
        End Function

#End Region

#Region "Obsolete"

        <Obsolete("This method has replaced in DotNetNuke 5.0 by DeserializeTab(ByVal nodeTab As XmlNode, ByVal objTab As TabInfo, ByVal PortalId As Integer, ByVal mergeTabs As PortalTemplateModuleAction)")> _
        Public Shared Function DeserializeTab(ByVal tabName As String, ByVal nodeTab As XmlNode, ByVal PortalId As Integer) As TabInfo
            Return TabController.DeserializeTab(nodeTab, Nothing, New Hashtable(), PortalId, False, PortalTemplateModuleAction.Ignore, New Hashtable())
        End Function

        <Obsolete("This method has replaced in DotNetNuke 5.0 by DeserializeTab(ByVal nodeTab As XmlNode, ByVal objTab As TabInfo, ByVal PortalId As Integer, ByVal mergeTabs As PortalTemplateModuleAction)")> _
        Public Shared Function DeserializeTab(ByVal nodeTab As XmlNode, ByVal objTab As TabInfo, ByVal PortalId As Integer) As TabInfo
            Return TabController.DeserializeTab(nodeTab, objTab, New Hashtable(), PortalId, False, PortalTemplateModuleAction.Ignore, New Hashtable())
        End Function

        <Obsolete("This method has replaced in DotNetNuke 5.0 by DeserializeTab(ByVal nodeTab As XmlNode, ByVal objTab As TabInfo, ByVal hTabs As Hashtable, ByVal PortalId As Integer, ByVal IsAdminTemplate As Boolean, ByVal mergeTabs As PortalTemplateModuleAction, ByVal hModules As Hashtable)")> _
        Public Shared Function DeserializeTab(ByVal tabName As String, ByVal nodeTab As XmlNode, ByVal objTab As TabInfo, ByVal hTabs As Hashtable, ByVal PortalId As Integer, ByVal IsAdminTemplate As Boolean, ByVal mergeTabs As PortalTemplateModuleAction, ByVal hModules As Hashtable) As TabInfo
            Return TabController.DeserializeTab(nodeTab, objTab, hTabs, PortalId, IsAdminTemplate, mergeTabs, hModules)
        End Function

        <Obsolete("This method has replaced in DotNetNuke 5.0 by CopyDesignToChildren(TabInfo,String, String)")> _
        Public Sub CopyDesignToChildren(ByVal tabs As ArrayList, ByVal skinSrc As String, ByVal containerSrc As String)
            For Each objTab As TabInfo In tabs
                provider.UpdateTab(objTab.TabID, objTab.ContentItemId, objTab.PortalID, objTab.TabName, objTab.IsVisible, objTab.DisableLink, _
                     objTab.ParentId, objTab.IconFile, objTab.IconFileLarge, objTab.Title, objTab.Description, objTab.KeyWords, _
                     objTab.IsDeleted, objTab.Url, skinSrc, containerSrc, objTab.TabPath, objTab.StartDate, _
                     objTab.EndDate, objTab.RefreshInterval, objTab.PageHeadText, objTab.IsSecure, objTab.PermanentRedirect, _
                     objTab.SiteMapPriority, UserController.GetCurrentUserInfo.UserID, Entities.Host.Host.ContentLocale.ToString)
                Dim objEventLog As New Services.Log.EventLog.EventLogController
                objEventLog.AddLog(objTab, PortalController.GetCurrentPortalSettings, UserController.GetCurrentUserInfo.UserID, "", Services.Log.EventLog.EventLogController.EventLogType.TAB_UPDATED)
            Next

            If tabs.Count > 0 Then
                DotNetNuke.Common.Utilities.DataCache.ClearTabsCache(CType(tabs(0), TabInfo).PortalID)
            End If
        End Sub

        <Obsolete("Deprecated in DotNetNuke 5.0. Replaced by CopyPermissionsToChildren(TabInfo, TabPermissionCollection)")> _
        Public Sub CopyPermissionsToChildren(ByVal tabs As ArrayList, ByVal newPermissions As Permissions.TabPermissionCollection)
            Dim objTabPermissionController As New Security.Permissions.TabPermissionController

            For Each objTab As TabInfo In tabs
                objTab.TabPermissions.Clear()
                objTab.TabPermissions.AddRange(newPermissions)

                TabPermissionController.SaveTabPermissions(objTab)
            Next

            If tabs.Count > 0 Then
                DotNetNuke.Common.Utilities.DataCache.ClearTabsCache(CType(tabs(0), TabInfo).PortalID)
            End If
        End Sub

        <Obsolete("This method is obsolete.  It has been replaced by GetTab(ByVal TabId As Integer, ByVal PortalId As Integer, ByVal ignoreCache As Boolean) ")> _
        Public Function GetTab(ByVal TabId As Integer) As TabInfo
            Return CBO.FillObject(Of TabInfo)(DataProvider.Instance().GetTab(TabId))
        End Function

        <Obsolete("This method has been replaced in 5.0 by GetTabPathDictionary(ByVal portalId As Integer) As Dictionary(Of String, Integer) ")> _
        Public Shared Function GetTabPathDictionary() As Dictionary(Of String, Integer)
            Dim tabpathDic As Dictionary(Of String, Integer) = New Dictionary(Of String, Integer)(StringComparer.CurrentCultureIgnoreCase)
            Dim dr As IDataReader = DataProvider.Instance().GetTabPaths(Null.NullInteger)
            Try
                While dr.Read
                    ' add to dictionary
                    Dim strKey As String = "//" & Null.SetNullInteger(dr("PortalID")) & Null.SetNullString(dr("TabPath"))
                    tabpathDic(strKey) = Null.SetNullInteger(dr("TabID"))
                End While
            Catch exc As Exception
                LogException(exc)
            Finally
                ' close datareader
                CBO.CloseDataReader(dr, True)
            End Try
            Return tabpathDic
        End Function

        <Obsolete("This method has replaced in DotNetNuke 5.0 by GetTabsByPortal()")> _
        Public Function GetTabs(ByVal PortalId As Integer) As ArrayList
            Return GetTabsByPortal(PortalId).ToArrayList()
        End Function

        <Obsolete("This method is obsolete.  It has been replaced by GetTabsByParent(ByVal ParentId As Integer, ByVal PortalId As Integer) ")> _
        Public Function GetTabsByParentId(ByVal ParentId As Integer) As ArrayList
            Return CBO.FillCollection((DataProvider.Instance().GetTabsByParentId(ParentId)), GetType(TabInfo))
        End Function

        <Obsolete("This method has replaced in DotNetNuke 5.0 by GetTabsByParent(ByVal ParentId As Integer, ByVal PortalId As Integer)")> _
        Public Function GetTabsByParentId(ByVal ParentId As Integer, ByVal PortalId As Integer) As ArrayList
            Dim arrTabs As New ArrayList
            For Each objTab As TabInfo In GetTabsByParent(ParentId, PortalId)
                arrTabs.Add(objTab)
            Next
            Return arrTabs
        End Function

        <Obsolete("This method is obsolete.  It has been replaced by UpdateTabOrder(ByVal objTab As TabInfo) ")> _
        Public Sub UpdateTabOrder(ByVal PortalID As Integer, ByVal TabId As Integer, ByVal TabOrder As Integer, ByVal Level As Integer, ByVal ParentId As Integer)
            Dim objTab As TabInfo = GetTab(TabId, PortalID, False)
            objTab.TabOrder = TabOrder
            objTab.Level = Level
            objTab.ParentId = ParentId
            UpdateTabOrder(objTab)
        End Sub


#End Region

    End Class


End Namespace
