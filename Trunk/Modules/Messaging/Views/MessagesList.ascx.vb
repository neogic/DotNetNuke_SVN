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

Imports DotNetNuke.Modules.Messaging.Presenters
Imports DotNetNuke.Web.Mvp.Framework
Imports DotNetNuke.Services.Localization
Imports DotNetNuke.Services.Messaging.Providers

Namespace DotNetNuke.Modules.Messaging.Views
    Partial Public Class MessagesList
        Inherits ViewBase(Of IMessagesListView, MessagesListPresenter, MessagesListPresenterModel)
        Implements IMessagesListView

#Region "Protected Properties"

        Protected Overrides ReadOnly Property View() As IMessagesListView
            Get
                Return Me
            End Get
        End Property

#End Region

#Region "ViewBase Virtual/Abstract Method Overrides"

        Protected Overrides Sub ConnectEvents()
            MyBase.ConnectEvents()
            AddHandler addMessageButton.Click, AddressOf Presenter.AddMessage
            AddHandler messagesGrid.ItemDataBound, AddressOf Presenter.MessageDataBound
        End Sub

        Protected Overrides Sub Localize()
            If Not IsPostBack Then
                Localization.LocalizeDataGrid(messagesGrid, LocalResourceFile)
            End If
        End Sub

#End Region

        Public Sub ShowMessages(ByVal messages As IEnumerable(Of Message)) Implements IMessagesListView.ShowMessages
            messagesGrid.DataSource = messages
            messagesGrid.DataBind()

            addMessageButton.Visible = Request.IsAuthenticated
        End Sub

    End Class
End Namespace
