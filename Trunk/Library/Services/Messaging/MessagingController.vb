'
' DotNetNukeŽ - http://www.dotnetnuke.com
' Copyright (c) 2002-2010
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

Imports DotNetNuke.Common.Utilities
Imports DotNetNuke.ComponentModel
Imports DotNetNuke.Services.Messaging.Data

Namespace DotNetNuke.Services.Messaging

    ''' -----------------------------------------------------------------------------
    ''' <summary>
    ''' The Controller class for Messaging
    ''' </summary>
    ''' <remarks>
    ''' </remarks>
    ''' <history>
    ''' </history>
    ''' -----------------------------------------------------------------------------
    Public Class MessagingController
        Implements IMessagingController

#Region "Private Members"

        Private _DataService As IMessagingDataService
        Private Shared _MessagingPage As DotNetNuke.Entities.Tabs.TabInfo = Nothing

#End Region

#Region "Constructors"

        Public Sub New()
            Me.New(GetDataService())
        End Sub

        Public Sub New(ByVal dataService As IMessagingDataService)
            _DataService = dataService
        End Sub

#End Region

#Region "Private Shared Methods"

        Private Shared Function GetDataService() As IMessagingDataService
            Dim ds As IMessagingDataService = ComponentFactory.GetComponent(Of IMessagingDataService)()

            If ds Is Nothing Then
                ds = New MessagingDataService()
                ComponentFactory.RegisterComponentInstance(Of IMessagingDataService)(ds)
            End If
            Return ds
        End Function

#End Region

#Region "Public Shared Methods"

        Public Shared ReadOnly Property DefaultMessagingURL(ByVal ModuleFriendlyName As String) As String
            Get
                Dim page As DotNetNuke.Entities.Tabs.TabInfo = MessagingPage(ModuleFriendlyName)
                If (Not (IsNothing(page))) Then
                    Return MessagingPage(ModuleFriendlyName).FullUrl
                Else
                    Return Nothing
                End If

            End Get
        End Property

        Public Shared Function MessagingPage(ByVal ModuleFriendlyName As String) As DotNetNuke.Entities.Tabs.TabInfo
            If (Not (_MessagingPage Is Nothing)) Then Return _MessagingPage

            Dim mc As New DotNetNuke.Entities.Modules.ModuleController
            Dim md As DotNetNuke.Entities.Modules.ModuleInfo = mc.GetModuleByDefinition(DotNetNuke.Entities.Portals.PortalSettings.Current.PortalId, ModuleFriendlyName)
            If Not (md Is Nothing) Then
                Dim a As ArrayList = mc.GetModuleTabs(md.ModuleID)
                If Not a Is Nothing Then
                    Dim mi As DotNetNuke.Entities.Modules.ModuleInfo = TryCast(a(0), DotNetNuke.Entities.Modules.ModuleInfo)
                    If Not (mi Is Nothing) Then
                        Dim tc As New DotNetNuke.Entities.Tabs.TabController
                        _MessagingPage = tc.GetTab(mi.TabID, DotNetNuke.Entities.Portals.PortalSettings.Current.PortalId, False)
                    End If
                End If
            End If

            Return _MessagingPage

        End Function

#End Region

#Region "Public Methods"

        Public Function GetMessageByID(ByVal PortalID As Integer, ByVal UserID As Integer, ByVal messageId As Integer) As Message Implements IMessagingController.GetMessageByID
            Return CType(CBO.FillObject(_DataService.GetMessageByID(messageId), GetType(Message)), Message)
        End Function

        Public Function GetUserInbox(ByVal PortalID As Integer, ByVal UserID As Integer, ByVal PageNumber As Integer, ByVal PageSize As Integer) As List(Of Message) Implements IMessagingController.GetUserInbox
            Return CBO.FillCollection(Of Message)(_DataService.GetUserInbox(PortalID, UserID, PageNumber, PageSize))
        End Function

        Public Function GetInboxCount(ByVal PortalID As Integer, ByVal UserID As Integer) As Integer Implements IMessagingController.GetInboxCount
            Return _DataService.GetInboxCount(PortalID, UserID)
        End Function

        Public Function GetNewMessageCount(ByVal PortalID As Integer, ByVal UserID As Integer) As Integer Implements IMessagingController.GetNewMessageCount
            Return _DataService.GetNewMessageCount(PortalID, UserID)
        End Function

        Public Function GetNextMessageForDispatch(ByVal SchedulerInstance As Guid) As Message Implements IMessagingController.GetNextMessageForDispatch
            Return CType(CBO.FillObject(_DataService.GetNextMessageForDispatch(SchedulerInstance), GetType(Message)), Message)
        End Function

        Public Sub SaveMessage(ByVal message As Message) Implements IMessagingController.SaveMessage

            If (PortalSettings.Current IsNot Nothing) Then
                message.PortalID = PortalSettings.Current.PortalId
            End If

            If (message.Conversation = Nothing Or message.Conversation = Guid.Empty) Then
                message.Conversation = Guid.NewGuid()
            End If

            _DataService.SaveMessage(message)
        End Sub

        Public Sub UpdateMessage(ByVal message As Message) Implements IMessagingController.UpdateMessage
            _DataService.UpdateMessage(message)
        End Sub

        Public Sub MarkMessageAsDispatched(ByVal MessageID As Integer) Implements IMessagingController.MarkMessageAsDispatched
            _DataService.MarkMessageAsDispatched(MessageID)
        End Sub


#End Region

    End Class
End Namespace
