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

Imports DotNetNuke.UI.WebControls

Namespace DotNetNuke.Security.Profile

    ''' -----------------------------------------------------------------------------
    ''' Project:    DotNetNuke
    ''' Namespace:  DotNetNuke.Security.Profile
    ''' Class:      ProfileProviderConfig
    ''' -----------------------------------------------------------------------------
    ''' <summary>
    ''' The ProfileProviderConfig class provides a wrapper to the Profile providers
    ''' configuration
    ''' </summary>
    ''' <remarks>
    ''' </remarks>
    ''' <history>
    '''     [cnurse]	03/09/2006	created
    ''' </history>
    ''' -----------------------------------------------------------------------------
    Public Class ProfileProviderConfig

#Region "Private Shared Members"

        Private Shared profileProvider As DotNetNuke.Security.Profile.ProfileProvider = DotNetNuke.Security.Profile.ProfileProvider.Instance()

#End Region

#Region "Public Shared Properties"

        ''' -----------------------------------------------------------------------------
        ''' <summary>
        ''' Gets whether the Provider Properties can be edited
        ''' </summary>
        ''' <returns>A Boolean</returns>
        ''' <history>
        '''     [cnurse]	03/02/2006	created
        ''' </history>
        ''' -----------------------------------------------------------------------------
        <Browsable(False)> Public Shared ReadOnly Property CanEditProviderProperties() As Boolean
            Get
                Return profileProvider.CanEditProviderProperties
            End Get
        End Property

#End Region

    End Class


End Namespace
