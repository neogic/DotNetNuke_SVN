
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

Namespace DotNetNuke.UI.Skins.EventListeners

    '''-----------------------------------------------------------------------------
    ''' <summary>
    ''' SkinEventArgs provides a custom EventARgs class for Skin Events
    ''' </summary>
    ''' <remarks></remarks>
    ''' <history>
    '''     [cnurse]	05/19/2009	Created
    ''' </history>
    '''-----------------------------------------------------------------------------
    Public Class SkinEventArgs
        Inherits EventArgs

#Region "Private Members"

        Private _Skin As Skin

#End Region

#Region "Constructors"

        Public Sub New(ByVal skin As Skin)
            _Skin = skin
        End Sub

#End Region

#Region "Public Properties"

        Public ReadOnly Property Skin() As Skin
            Get
                Return _Skin
            End Get
        End Property

#End Region

    End Class

End Namespace
