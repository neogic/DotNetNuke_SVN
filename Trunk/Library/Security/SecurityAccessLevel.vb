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
Imports System.Collections
Imports System.Configuration
Imports System.Data
Imports System.IO
Imports System.Security.Cryptography
Imports System.Text
Imports System.Text.RegularExpressions
Imports System.Web
Imports System.Web.Security

Imports DotNetNuke.Entities.Modules
Imports DotNetNuke.Security.Membership
Imports DotNetNuke.Security.Permissions

Namespace DotNetNuke.Security

    '''-----------------------------------------------------------------------------
    ''' <summary>
    ''' The SecurityAccessLevel enum is used to determine which level of access rights
    ''' to assign to a specific module or module action. 
    ''' </summary>
    '''-----------------------------------------------------------------------------
    Public Enum SecurityAccessLevel As Integer
        ControlPanel = -3
        SkinObject = -2
        Anonymous = -1
        View = 0
        Edit = 1
        Admin = 2
        Host = 3
    End Enum

End Namespace