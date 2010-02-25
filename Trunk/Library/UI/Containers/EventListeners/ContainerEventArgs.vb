
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

Namespace DotNetNuke.UI.Containers.EventListeners

    '''-----------------------------------------------------------------------------
    ''' <summary>
    ''' ContainerEventArgs provides a custom EventARgs class for Container Events
    ''' </summary>
    ''' <remarks></remarks>
    ''' <history>
    '''     [cnurse]	05/20/2009	Created
    ''' </history>
    '''-----------------------------------------------------------------------------
    Public Class ContainerEventArgs
        Inherits EventArgs

#Region "Private Members"

        Private _Container As Container

#End Region

#Region "Constructors"

        Public Sub New(ByVal container As Container)
            _Container = container
        End Sub

#End Region

#Region "Public Properties"

        Public ReadOnly Property Container() As Container
            Get
                Return _Container
            End Get
        End Property

#End Region

    End Class

End Namespace
