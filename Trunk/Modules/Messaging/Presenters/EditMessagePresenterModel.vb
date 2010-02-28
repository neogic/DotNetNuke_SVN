'
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

Imports DotNetNuke.Common.Utilities
Imports DotNetNuke.Web.Mvp.Framework

Namespace DotNetNuke.Modules.Messaging.Presenters
    Public Class EditMessagePresenterModel
        Inherits MessagingPresenterModelBase

        Private _OriginalIndexId As Integer = Null.NullInteger
        Private Const _OriginalIndexIdQueryString As String = "OriginalIndexId"

#Region "Public Properties"

        Public Property OriginalIndexId() As Integer
            Get
                Return _OriginalIndexId
            End Get
            Set(ByVal value As Integer)
                _OriginalIndexId = value
            End Set
        End Property

        Public Overrides ReadOnly Property PresenterName() As String
            Get
                Return EditMessagePresenter.Name
            End Get
        End Property

#End Region

#Region "Virtual Method Overrides"

        Public Overrides Sub BuildQueryString(ByVal queryString As System.Collections.Specialized.NameValueCollection)
            MyBase.BuildQueryString(queryString)
            queryString.AddIfNotNull(_OriginalIndexIdQueryString, OriginalIndexId)
        End Sub

        Protected Overrides Sub LoadFromQueryString(ByVal parameters As System.Collections.Specialized.NameValueCollection)
            MyBase.LoadFromQueryString(parameters)
            OriginalIndexId = parameters.ToInt32(_OriginalIndexIdQueryString)
        End Sub

#End Region

    End Class
End Namespace
