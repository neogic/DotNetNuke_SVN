'
' DotNetNuke� - http://www.dotnetnuke.com
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

Namespace DotNetNuke.Services.Installer.Packages

    '''-----------------------------------------------------------------------------
    ''' Project		: DotNetNuke
    ''' Namespace   : DotNetNuke.Services.Installer.Packages
    ''' Class		: PackageCreatedEventArgs
    '''-----------------------------------------------------------------------------
    ''' <summary>
    ''' PackageCreatedEventArgs provides a custom EventArgs class for a
    ''' Package Created Event.
    ''' </summary>
    ''' <history>
    '''     [cnurse]	01/23/2008	Created
    ''' </history>
    '''-----------------------------------------------------------------------------
    Public Class PackageCreatedEventArgs
        Inherits EventArgs

#Region "Private Members"

        Private _Package As PackageInfo

#End Region

#Region "Constructors"

        '''-----------------------------------------------------------------------------
        ''' <summary>
        ''' Builds a new PackageCreatedEventArgs
        ''' </summary>
        ''' <param name="package">The package associated with this event</param>
        ''' <remarks></remarks>
        ''' <history>
        '''     [cnurse]	01/23/2008	Created
        ''' </history>
        '''-----------------------------------------------------------------------------
        Public Sub New(ByVal package As PackageInfo)
            _Package = package
        End Sub

#End Region

#Region "Public Properties"

        '''-----------------------------------------------------------------------------
        ''' <summary>
        ''' Gets the Package associated with this event
        ''' </summary>
        ''' <history>
        '''     [cnurse]	01/23/2008	Created
        ''' </history>
        '''-----------------------------------------------------------------------------
        Public ReadOnly Property Package() As PackageInfo
            Get
                Return _Package
            End Get
        End Property

#End Region

    End Class

End Namespace
