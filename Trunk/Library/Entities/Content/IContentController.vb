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

Imports System.Collections.Specialized

Namespace DotNetNuke.Entities.Content
    Public Interface IContentController
        Function AddContentItem(ByVal contentItem As ContentItem) As Integer
        Sub DeleteContentItem(ByVal contentItem As ContentItem)
        Function GetContentItem(ByVal contentItemId As Integer) As ContentItem
        Function GetContentItemsByTerm(ByVal term As String) As IQueryable(Of ContentItem)
        Function GetUnIndexedContentItems() As IQueryable(Of ContentItem)
        Sub UpdateContentItem(ByVal contentItem As ContentItem)

        Sub AddMetaData(ByVal contentItem As ContentItem, ByVal name As String, ByVal value As String)
        Sub DeleteMetaData(ByVal contentItem As ContentItem, ByVal name As String, ByVal value As String)
        Function GetMetaData(ByVal contentItemId As Integer) As NameValueCollection

    End Interface
End Namespace