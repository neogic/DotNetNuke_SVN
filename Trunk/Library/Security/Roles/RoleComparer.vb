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

Namespace DotNetNuke.Security.Roles

    ''' -----------------------------------------------------------------------------
    ''' Project:    DotNetNuke
    ''' Namespace:  DotNetNuke.Security.Roles
    ''' Class:      RoleComparer
    ''' -----------------------------------------------------------------------------
    ''' <summary>
    ''' The RoleComparer class provides an Implementation of IComparer for 
    ''' RoleInfo objects
    ''' </summary>
    ''' <history>
    '''     [cnurse]    05/24/2005  Split into separate file and documented
    ''' </history>
    ''' -----------------------------------------------------------------------------
    Public Class RoleComparer
        Implements IComparer

        ''' -----------------------------------------------------------------------------
        ''' <summary>
        ''' Compares two RoleInfo objects by performing a comparison of their rolenames
        ''' </summary>
        ''' <param name="x">One of the items to compare</param>
        ''' <param name="y">One of the items to compare</param>
        ''' <returns>An Integer that determines whether x is greater, smaller or equal to y </returns>
        ''' <history>
        ''' 	[cnurse]	05/24/2005	Documented
        ''' </history>
        ''' -----------------------------------------------------------------------------
        Public Function Compare(ByVal x As Object, ByVal y As Object) As Integer Implements IComparer.Compare
            Return New CaseInsensitiveComparer().Compare(DirectCast(x, RoleInfo).RoleName, DirectCast(y, RoleInfo).RoleName)
        End Function

    End Class
End Namespace
