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

Imports System.Web.Caching
Imports DotNetNuke.Entities.Content.Common
Imports DotNetNuke.Entities.Content.Data
Imports DotNetNuke.Entities.Users
Imports DotNetNuke.Common.Utilities

Namespace DotNetNuke.Entities.Content.Taxonomy
    Public Interface ITermController
        Function AddTerm(ByVal term As Term) As Integer
        Sub AddTermToContent(ByVal term As Term, ByVal contentItem As ContentItem)
        Sub DeleteTerm(ByVal term As Term)
        Function GetTerm(ByVal termId As Integer) As Term
        Function GetTermsByContent(ByVal contentItemId As Integer) As IQueryable(Of Term)
        'Function GetTermsByPortal(ByVal portalId As Integer) As IQueryable(Of Term)
        Function GetTermsByVocabulary(ByVal vocabularyId As Integer) As IQueryable(Of Term)
        Sub RemoveTermsFromContent(ByVal contentItem As ContentItem)
        Sub UpdateTerm(ByVal term As Term)
    End Interface
End Namespace