﻿'
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

Namespace DotNetNuke.Web.UI.WebControls

    Public Class DnnRadioButton
        Inherits System.Web.UI.WebControls.RadioButton
        Implements DotNetNuke.Web.UI.ILocalize

		Public Sub New()
			CssClass = "SubHead dnnLabel"
		End Sub

        Protected Overrides Sub Render(ByVal writer As System.Web.UI.HtmlTextWriter)
            LocalizeStrings()
            MyBase.Render(writer)
        End Sub

#Region "Implements ILocalize"

		Private _Localize As Boolean = True
		Public Property Localize() As Boolean Implements ILocalize.Localize
			Get
				Return _Localize
			End Get
			Set(ByVal value As Boolean)
				_Localize = value
			End Set
		End Property

		Protected Overridable Sub LocalizeStrings() Implements ILocalize.LocalizeStrings
			If (Localize) Then
				If (Not String.IsNullOrEmpty(ToolTip)) Then
					ToolTip = Utilities.GetLocalizedStringFromParent(ToolTip, Me)
				End If

				If (Not String.IsNullOrEmpty(Text)) Then
					Text = Utilities.GetLocalizedStringFromParent(Text, Me)

					If (String.IsNullOrEmpty(ToolTip)) Then
						ToolTip = Utilities.GetLocalizedStringFromParent(Text + ".ToolTip", Me)
					End If
				End If
			End If
		End Sub

#End Region

    End Class

End Namespace
