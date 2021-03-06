﻿'
' DotNetNuke - http://www.dotnetnuke.com
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

Imports System.Collections.Generic

Imports DotNetNuke.Common
Imports DotNetNuke.Common.Utilities
Imports DotNetNuke.Entities.Users
Imports DotNetNuke.Entities.Content.Common
Imports DotNetNuke.Entities.Content.Data
Imports System.Collections.Specialized

Namespace DotNetNuke.Entities.Content

    Public Class ContentController
        Implements IContentController

#Region "Private Members"

        Private _DataService As IDataService

#End Region

#Region "Constructors"

        Public Sub New()
            Me.New(Util.GetDataService())
        End Sub

        Public Sub New(ByVal dataService As IDataService)
            _DataService = dataService
        End Sub

#End Region

#Region "Public Methods"

        Public Function AddContentItem(ByVal contentItem As ContentItem) As Integer Implements IContentController.AddContentItem
            'Argument Contract
            Requires.NotNull("contentItem", contentItem)

            contentItem.ContentItemId = _DataService.AddContentItem(contentItem, UserController.GetCurrentUserInfo.UserID)

            Return contentItem.ContentItemId
        End Function

        Public Sub DeleteContentItem(ByVal contentItem As ContentItem) Implements IContentController.DeleteContentItem
            'Argument Contract
            Requires.NotNull("contentItem", contentItem)
            Requires.PropertyNotNegative("contentItem", "ContentItemId", contentItem.ContentItemId)

            _DataService.DeleteContentItem(contentItem)
        End Sub

        Public Function GetContentItem(ByVal contentItemId As Integer) As ContentItem Implements IContentController.GetContentItem
            'Argument Contract
            Requires.NotNegative("contentItemId", contentItemId)

            Return CBO.FillObject(Of ContentItem)(_DataService.GetContentItem(contentItemId))
        End Function

        Public Function GetContentItemsByTerm(ByVal term As String) As IQueryable(Of ContentItem) Implements IContentController.GetContentItemsByTerm
            'Argument Contract
            Requires.NotNullOrEmpty("term", term)

            Return CBO.FillQueryable(Of ContentItem)(_DataService.GetContentItemsByTerm(term))
        End Function

        Public Function GetUnIndexedContentItems() As IQueryable(Of ContentItem) Implements IContentController.GetUnIndexedContentItems
            Return CBO.FillQueryable(Of ContentItem)(_DataService.GetUnIndexedContentItems())
        End Function

        Public Sub UpdateContentItem(ByVal contentItem As ContentItem) Implements IContentController.UpdateContentItem
            'Argument Contract
            Requires.NotNull("contentItem", contentItem)
            Requires.PropertyNotNegative("contentItem", "ContentItemId", contentItem.ContentItemId)

            _DataService.UpdateContentItem(contentItem, UserController.GetCurrentUserInfo.UserID)
        End Sub



        Public Sub AddMetaData(ByVal contentItem As ContentItem, ByVal name As String, ByVal value As String) Implements IContentController.AddMetaData
            'Argument Contract
            Requires.NotNull("contentItem", contentItem)
            Requires.PropertyNotNegative("contentItem", "ContentItemId", contentItem.ContentItemId)
            Requires.NotNullOrEmpty("name", name)

            _DataService.AddMetaData(contentItem, name, value)
        End Sub

        Public Sub DeleteMetaData(ByVal contentItem As ContentItem, ByVal name As String, ByVal value As String) Implements IContentController.DeleteMetaData
            'Argument Contract
            Requires.NotNull("contentItem", contentItem)
            Requires.PropertyNotNegative("contentItem", "ContentItemId", contentItem.ContentItemId)
            Requires.NotNullOrEmpty("name", name)

            _DataService.DeleteMetaData(contentItem, name, value)
        End Sub

        Public Function GetMetaData(ByVal contentItemId As Integer) As NameValueCollection Implements IContentController.GetMetaData
            'Argument Contract
            Requires.NotNegative("contentItemId", contentItemId)

            Dim metadata As New NameValueCollection()
            Dim dr As IDataReader = _DataService.GetMetaData(contentItemId)
            While dr.Read()
                metadata.Add(dr.GetString(0), dr.GetString(1))
            End While

            Return metadata
        End Function

#End Region

    End Class

End Namespace

