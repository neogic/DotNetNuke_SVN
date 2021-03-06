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
Imports System.IO
Imports System.Xml.XPath

Imports DotNetNuke.Services.Localization
Imports System.Xml
Imports DotNetNuke.Services.Installer.Packages
Imports DotNetNuke.Services.Authentication


Namespace DotNetNuke.Services.Installer.Writers

    ''' -----------------------------------------------------------------------------
    ''' <summary>
    ''' The ProviderPackageWriter class 
    ''' </summary>
    ''' <remarks>
    ''' </remarks>
    ''' <history>
    ''' 	[cnurse]	05/29/2008	created
    ''' </history>
    ''' -----------------------------------------------------------------------------
    Public Class ProviderPackageWriter
        Inherits PackageWriterBase

#Region "Constructors"

        Public Sub New(ByVal package As PackageInfo)
            MyBase.New(package)
            Dim configDoc As XmlDocument = Config.Load()
            Dim providerNavigator As XPathNavigator = configDoc.CreateNavigator.SelectSingleNode("/configuration/dotnetnuke/*/providers/add[@name='" & package.Name & "']")
            Dim providerPath As String = Null.NullString

            If providerNavigator IsNot Nothing Then
                providerPath = Util.ReadAttribute(providerNavigator, "providerPath")
            End If

            If Not String.IsNullOrEmpty(providerPath) Then
                BasePath = providerPath.Replace("~/", "").Replace("/", "\")
            End If
        End Sub

#End Region

        Protected Overrides Sub GetFiles(ByVal includeSource As Boolean, ByVal includeAppCode As Boolean)
            MyBase.GetFiles(includeSource, False)
        End Sub

    End Class

End Namespace

