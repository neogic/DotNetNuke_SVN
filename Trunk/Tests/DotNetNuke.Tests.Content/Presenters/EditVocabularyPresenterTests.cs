﻿// '
// ' DotNetNuke® - http://www.dotnetnuke.com
// ' Copyright (c) 2002-2010
// ' by DotNetNuke Corporation
// '
// ' Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// ' documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// ' the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// ' to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// '
// ' The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// ' of the Software.
// '
// ' THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// ' TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// ' THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// ' CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// ' DEALINGS IN THE SOFTWARE.
// '
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Web;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Content.Taxonomy;
using DotNetNuke.Modules.Taxonomy.Presenters;
using DotNetNuke.Modules.Taxonomy.Views;
using DotNetNuke.Modules.Taxonomy.Views.Models;
using DotNetNuke.Tests.Content.Mocks;
using DotNetNuke.Tests.Utilities;
using DotNetNuke.Tests.Utilities.Mocks;
using DotNetNuke.Web.UI.WebControls;
using DotNetNuke.Web.Validators;
using MbUnit.Framework;
using Moq;

namespace DotNetNuke.Tests.Content.Presenters
{
    /// <summary>
    /// Summary description for VocabulariesListPresenter Tests
    /// </summary>
    [TestFixture]
    public class EditVocabularyPresenterTests
    {
        [TearDown]
        public void TearDown()
        {
            MockComponentProvider.ResetContainer();
        }

        #region Constructor Tests

        [Test]
        public void EditVocabularyPresenter_Constructor_Requires_Non_Null_VocabularyController()
        {
            //Arrange
            Mock<IEditVocabularyView> view = new Mock<IEditVocabularyView>();
            Mock<ITermController> termController = new Mock<ITermController>();

            //Act, Assert
            AutoTester.ArgumentNull<IVocabularyController>(
                v => new EditVocabularyPresenter(view.Object, v, termController.Object));
        }

        [Test]
        public void EditVocabularyPresenter_Constructor_Requires_Non_Null_TermController()
        {
            //Arrange
            Mock<IEditVocabularyView> view = new Mock<IEditVocabularyView>();
            Mock<IVocabularyController> vocabularyController = new Mock<IVocabularyController>();

            //Act, Assert
            AutoTester.ArgumentNull<ITermController>(
                t => new EditVocabularyPresenter(view.Object, vocabularyController.Object, t));
        }

        #endregion

        #region Initialization Tests

        [Test]
        public void EditVocabularyPresenter_OnInit_Calls_VocabularyController_GetVocabularies()
        {
            // Arrange
            Mock<IEditVocabularyView> mockView = new Mock<IEditVocabularyView>();
            mockView.Setup(v => v.Model).Returns(new EditVocabularyModel());

            EditVocabularyPresenter presenter = CreatePresenter(mockView, false);

            // Act (Raise the Initialize Event)
            mockView.Raise(v => v.Initialize += null, EventArgs.Empty);

            // Assert
            Mock.Get(presenter.VocabularyController)
                .Verify(c => c.GetVocabularies());
        }

        [Test]
        public void EditVocabularyPresenter_OnInit_Calls_Controller_Gets_Vocabulary_With_Correct_Id()
        {
            // Arrange
            Mock<IEditVocabularyView> mockView = new Mock<IEditVocabularyView>();
            mockView.Setup(v => v.Model).Returns(new EditVocabularyModel());

            EditVocabularyPresenter presenter = CreatePresenter(mockView, false);

            // Act (Raise the Initialize Event)
            mockView.Raise(v => v.Initialize += null, EventArgs.Empty);

            // Assert
            Assert.AreEqual(Constants.VOCABULARY_ValidVocabularyId, mockView.Object.Model.Vocabulary.VocabularyId);
        }

        [Test]
        public void EditVocabularyPresenter_OnInit_Calls_TermController_GetTermsByVocabulary_With_Correct_Id()
        {
            // Arrange
            Mock<IEditVocabularyView> mockView = new Mock<IEditVocabularyView>();
            mockView.Setup(v => v.Model).Returns(new EditVocabularyModel());

            EditVocabularyPresenter presenter = CreatePresenter(mockView, false);

            // Act (Raise the Initialize Event)
            mockView.Raise(v => v.Initialize += null, EventArgs.Empty);

            // Assert
            Mock.Get(presenter.TermController)
                .Verify(c => c.GetTermsByVocabulary(Constants.VOCABULARY_ValidVocabularyId));
        }

        #endregion

        #region View Load Tests

        [Test]
        [Row(true, true)]
        [Row(true, false)]
        [Row(false, true)]
        [Row(false, false)]
        public void EditVocabularyPresenter_OnLoad_Calls_View_BindVocabulary(bool isSuperUser, bool isSystem)
        {
            // Arrange
            Mock<IEditVocabularyView> mockView = new Mock<IEditVocabularyView>();
            EditVocabularyModel editModel = new EditVocabularyModel
                                                {
                                                    Vocabulary = new Vocabulary
                                                                     {
                                                                         VocabularyId =
                                                                             Constants.VOCABULARY_UpdateVocabularyId,
                                                                         ScopeTypeId = 1,
                                                                         IsSystem = isSystem // Set the IsSytem property
                                                                     }
                                                };
            mockView.Setup(v => v.Model).Returns(editModel);

            EditVocabularyPresenter presenter = CreatePresenter(mockView, false);
            presenter.IsSuperUser = isSuperUser;
            mockView.Raise(v => v.Initialize += null, EventArgs.Empty);

            // Act (Raise the Load Event)
            mockView.Raise(v => v.Load += null, EventArgs.Empty);

            // Assert
            mockView.Verify(
                v =>
                v.BindVocabulary(It.Is<Vocabulary>(vm => vm.VocabularyId == Constants.VOCABULARY_UpdateVocabularyId),
                                 presenter.IsEditEnabled,
                                 presenter.IsDeleteEnabled,
                                 isSuperUser));
        }

        [Test]
        [Row(true)]
        [Row(false)]
        public void EditVocabularyPresenter_OnLoad_Calls_View_BindTerms(bool isPostBack)
        {
            // Arrange
            Mock<IEditVocabularyView> mockView = new Mock<IEditVocabularyView>();
            mockView.Setup(v => v.Model).Returns(new EditVocabularyModel());

            EditVocabularyPresenter presenter = CreatePresenter(mockView, false);
            presenter.IsPostBack = isPostBack;
            mockView.Raise(v => v.Initialize += null, EventArgs.Empty);

            // Act (Raise the Load Event)
            mockView.Raise(v => v.Load += null, EventArgs.Empty);

            // Assert
            mockView.Verify(v => v.BindTerms(It.IsAny<List<Term>>(), presenter.IsHeirarchical, !isPostBack));
        }

        #endregion

        #region AddTerm Tests

        [Test]
        public void EditVocabularyPresenter_AddTerm_Loads_New_Term()
        {
            // Arrange
            Mock<IEditVocabularyView> mockView = new Mock<IEditVocabularyView>();
            mockView.Setup(v => v.Model).Returns(new EditVocabularyModel());

            EditVocabularyPresenter presenter = CreatePresenter(mockView, true);

            // Act
            mockView.Raise(v => v.AddTerm += null, EventArgs.Empty);

            // Assert
            mockView.Verify(v => v.BindTerm(It.Is<Term>(t => t.TermId == Null.NullInteger),
                                            It.IsAny<IEnumerable<Term>>(),
                                            presenter.IsHeirarchical,
                                            false,
                                            false));
        }

        [Test]
        public void EditVocabularyPresenter_AddTerm_Displays_Term_Editor()
        {
            // Arrange
            Mock<IEditVocabularyView> mockView = new Mock<IEditVocabularyView>();
            mockView.Setup(v => v.Model).Returns(new EditVocabularyModel());

            EditVocabularyPresenter presenter = CreatePresenter(mockView, true);

            // Act
            mockView.Raise(v => v.AddTerm += null, EventArgs.Empty);

            // Assert
            mockView.Verify(v => v.ShowTermEditor(true));
        }

        [Test]
        public void EditVocabularyPresenter_AddTerm_Correctly_Sets_TermEditor_Mode()
        {
            // Arrange
            Mock<IEditVocabularyView> mockView = new Mock<IEditVocabularyView>();
            mockView.Setup(v => v.Model).Returns(new EditVocabularyModel());

            EditVocabularyPresenter presenter = CreatePresenter(mockView, true);

            // Act
            mockView.Raise(v => v.AddTerm += null, EventArgs.Empty);

            // Assert
            mockView.Verify(v => v.SetTermEditorMode(true, Null.NullInteger));
        }

        #endregion

        #region Cancel Tests

        [Test]
        public void EditVocabularyPresenter_Cancel_Redirects_To_Vocabulary_List_View()
        {
            // Arrange
            Mock<IEditVocabularyView> mockView = new Mock<IEditVocabularyView>();
            mockView.Setup(v => v.Model).Returns(new EditVocabularyModel());

            Mock<HttpResponseBase> mockHttpResponse = new Mock<HttpResponseBase>();

            EditVocabularyPresenter presenter = CreatePresenter(mockView, mockHttpResponse, true);
            presenter.TabId = Constants.TAB_ValidId;

            // Act
            mockView.Raise(v => v.Cancel += null, EventArgs.Empty);

            // Assert
            mockHttpResponse.Verify(r => r.Redirect(Globals.NavigateURL(Constants.TAB_ValidId)));
        }

        #endregion

        #region CancelTerm Tests

        [Test]
        public void EditVocabularyPresenter_CancelTerm_Hides_Term_Editor()
        {
            // Arrange
            Mock<IEditVocabularyView> mockView = new Mock<IEditVocabularyView>();
            mockView.Setup(v => v.Model).Returns(new EditVocabularyModel());

            EditVocabularyPresenter presenter = CreatePresenter(mockView, true);

            // Act
            mockView.Raise(v => v.CancelTerm += null, EventArgs.Empty);

            // Assert
            mockView.Verify(v => v.ShowTermEditor(false));
        }

        [Test]
        public void EditVocabularyPresenter_CancelTerm_Clears_SelectedTerm()
        {
            // Arrange
            Mock<IEditVocabularyView> mockView = new Mock<IEditVocabularyView>();
            mockView.Setup(v => v.Model).Returns(new EditVocabularyModel());

            EditVocabularyPresenter presenter = CreatePresenter(mockView, true);

            // Act
            mockView.Raise(v => v.CancelTerm += null, EventArgs.Empty);

            // Assert
            mockView.Verify(v => v.ClearSelectedTerm());
        }

        #endregion

        #region DeleteTerm Tests

        [Test]
        public void EditVocabularyPresenter_DeleteTerm_Calls_TermController_DeleteTerm()
        {
            // Arrange
            Mock<IEditVocabularyView> mockView = new Mock<IEditVocabularyView>();

            EditVocabularyModel editModel = new EditVocabularyModel
                                                {
                                                    Term = new Term {TermId = Constants.TERM_DeleteTermId}
                                                };
            mockView.Setup(v => v.Model).Returns(editModel);

            EditVocabularyPresenter presenter = CreatePresenter(mockView, true);

            // Act
            mockView.Raise(v => v.DeleteTerm += null, EventArgs.Empty);

            // Assert
            Mock.Get(presenter.TermController)
                .Verify(r => r.DeleteTerm(It.Is<Term>(t => t.TermId == Constants.TERM_DeleteTermId)));
        }

        [Test]
        public void EditVocabularyPresenter_DeleteTerm_Clears_SelectedTerm()
        {
            // Arrange
            Mock<IEditVocabularyView> mockView = new Mock<IEditVocabularyView>();
            mockView.Setup(v => v.Model).Returns(new EditVocabularyModel());

            EditVocabularyPresenter presenter = CreatePresenter(mockView, true);

            // Act
            mockView.Raise(v => v.DeleteTerm += null, EventArgs.Empty);

            // Assert
            mockView.Verify(v => v.ClearSelectedTerm());
        }

        [Test]
        public void EditVocabularyPresenter_DeleteTerm_Refreshes_Terms()
        {
            // Arrange
            Mock<IEditVocabularyView> mockView = new Mock<IEditVocabularyView>();
            mockView.Setup(v => v.Model).Returns(new EditVocabularyModel());

            EditVocabularyPresenter presenter = CreatePresenter(mockView, true);

            // Act
            mockView.Raise(v => v.DeleteTerm += null, EventArgs.Empty);

            // Assert
            mockView.Verify(v => v.BindTerms(It.IsAny<IEnumerable<Term>>(),
                                             presenter.IsHeirarchical,
                                             true));
        }

        [Test]
        public void EditVocabularyPresenter_DeleteTerm_Hides_Term_Editor()
        {
            // Arrange
            Mock<IEditVocabularyView> mockView = new Mock<IEditVocabularyView>();
            mockView.Setup(v => v.Model).Returns(new EditVocabularyModel());

            EditVocabularyPresenter presenter = CreatePresenter(mockView, true);

            // Act
            mockView.Raise(v => v.DeleteTerm += null, EventArgs.Empty);

            // Assert
            mockView.Verify(v => v.ShowTermEditor(false));
        }

        #endregion

        #region DeleteVocabulary Tests

        [Test]
        public void EditVocabularyPresenter_DeleteVocabulary_Deletes_Vocabulary()
        {
            // Arrange
            Mock<IEditVocabularyView> mockView = new Mock<IEditVocabularyView>();
            mockView.Setup(v => v.Model).Returns(new EditVocabularyModel());

            EditVocabularyPresenter presenter = CreatePresenter(mockView, Constants.VOCABULARY_DeleteVocabularyId, true);

            // Act
            mockView.Raise(v => v.Delete += null, EventArgs.Empty);

            // Assert
            Mock.Get(presenter.VocabularyController)
                .Verify(
                    r =>
                    r.DeleteVocabulary(It.Is<Vocabulary>(v => v.VocabularyId == Constants.VOCABULARY_DeleteVocabularyId)));
        }

        [Test]
        public void EditVocabularyPresenter_DeleteVocabulary_Redirects_To_Vocabulary_List_View()
        {
            // Arrange
            Mock<IEditVocabularyView> mockView = new Mock<IEditVocabularyView>();
            mockView.Setup(v => v.Model).Returns(new EditVocabularyModel());

            Mock<HttpResponseBase> mockHttpResponse = new Mock<HttpResponseBase>();

            EditVocabularyPresenter presenter = CreatePresenter(mockView, mockHttpResponse,
                                                                Constants.VOCABULARY_DeleteVocabularyId, true);
            presenter.TabId = Constants.TAB_ValidId;

            // Act
            mockView.Raise(v => v.Delete += null, EventArgs.Empty);

            // Assert
            mockHttpResponse.Verify(r => r.Redirect(Globals.NavigateURL(Constants.TAB_ValidId)));
        }

        #endregion

        #region SaveTerm Tests

        [Test]
        public void EditVocabularyPresenter_SaveTerm_Validates_Term()
        {
            // Arrange
            Mock<IEditVocabularyView> mockView = new Mock<IEditVocabularyView>();

            EditVocabularyModel editModel = new EditVocabularyModel
                                                {
                                                    Term = new Term {TermId = Constants.TERM_UpdateTermId}
                                                };
            mockView.Setup(v => v.Model).Returns(editModel);

            EditVocabularyPresenter presenter = CreatePresenter(mockView, true);

            Mock<ObjectValidator> mockValidator = MockHelper.EnableValidMockValidator(presenter.Validator,
                                                                                      editModel.Term);

            // Act
            mockView.Raise(v => v.SaveTerm += null, EventArgs.Empty);

            // Assert
            mockValidator.Verify(v => v.ValidateObject(It.IsAny<Term>()));
        }

        [Test]
        public void EditVocabularyPresenter_SaveTerm_Does_Not_Save_If_Term_Invalid()
        {
            // Arrange
            Mock<IEditVocabularyView> mockView = new Mock<IEditVocabularyView>();

            EditVocabularyModel editModel = new EditVocabularyModel
                                                {
                                                    Term = new Term {TermId = Constants.TERM_UpdateTermId}
                                                };
            mockView.Setup(v => v.Model).Returns(editModel);

            EditVocabularyPresenter presenter = CreatePresenter(mockView, true);

            Mock<ObjectValidator> mockValidator = MockHelper.EnableInvalidMockValidator(presenter.Validator,
                                                                                        editModel.Term);

            // Act
            mockView.Raise(v => v.SaveTerm += null, EventArgs.Empty);

            // Assert
            Mock.Get(presenter.TermController)
                .Verify(r => r.UpdateTerm(It.IsAny<Term>()), Times.Never());
        }

        [Test]
        public void EditVocabularyPresenter_SaveTerm_Adds_New_Term_If_TermId_Negative_1()
        {
            // Arrange
            Mock<IEditVocabularyView> mockView = new Mock<IEditVocabularyView>();

            EditVocabularyModel editModel = new EditVocabularyModel
                                                {
                                                    Term = new Term()
                                                };
            mockView.Setup(v => v.Model).Returns(editModel);

            EditVocabularyPresenter presenter = CreatePresenter(mockView, true);

            // Act
            mockView.Raise(v => v.SaveTerm += null, EventArgs.Empty);

            // Assert
            Mock.Get(presenter.TermController)
                .Verify(r => r.AddTerm(It.Is<Term>(t => t.TermId == Null.NullInteger)));
        }

        [Test]
        public void EditVocabularyPresenter_SaveTerm_Updates_Term_If_Term_Valid()
        {
            // Arrange
            Mock<IEditVocabularyView> mockView = new Mock<IEditVocabularyView>();

            EditVocabularyModel editModel = new EditVocabularyModel
                                                {
                                                    Term = new Term {TermId = Constants.TERM_UpdateTermId}
                                                };
            mockView.Setup(v => v.Model).Returns(editModel);

            EditVocabularyPresenter presenter = CreatePresenter(mockView, true);

            // Act
            mockView.Raise(v => v.SaveTerm += null, EventArgs.Empty);

            // Assert
            Mock.Get(presenter.TermController)
                .Verify(r => r.UpdateTerm(It.Is<Term>(t => t.TermId == Constants.TERM_UpdateTermId)));
        }

        [Test]
        public void EditVocabularyPresenter_SaveTerm_Clears_SelectedTerm()
        {
            // Arrange
            Mock<IEditVocabularyView> mockView = new Mock<IEditVocabularyView>();
            EditVocabularyModel editModel = new EditVocabularyModel
                                                {
                                                    Term = new Term {TermId = Constants.TERM_UpdateTermId}
                                                };
            mockView.Setup(v => v.Model).Returns(editModel);

            EditVocabularyPresenter presenter = CreatePresenter(mockView, true);

            // Act
            mockView.Raise(v => v.SaveTerm += null, EventArgs.Empty);

            // Assert
            mockView.Verify(v => v.ClearSelectedTerm());
        }

        [Test]
        public void EditVocabularyPresenter_SaveTerm_Refreshes_Terms_List()
        {
            // Arrange
            Mock<IEditVocabularyView> mockView = new Mock<IEditVocabularyView>();
            EditVocabularyModel editModel = new EditVocabularyModel
                                                {
                                                    Term = new Term {TermId = Constants.TERM_UpdateTermId}
                                                };
            mockView.Setup(v => v.Model).Returns(editModel);

            EditVocabularyPresenter presenter = CreatePresenter(mockView, true);

            // Act
            mockView.Raise(v => v.SaveTerm += null, EventArgs.Empty);

            // Assert
            mockView.Verify(v => v.BindTerms(It.IsAny<IEnumerable<Term>>(),
                                             presenter.IsHeirarchical,
                                             true));
        }

        [Test]
        public void EditVocabularyPresenter_SaveTerm_Hides_Term_Editor()
        {
            // Arrange
            Mock<IEditVocabularyView> mockView = new Mock<IEditVocabularyView>();
            EditVocabularyModel editModel = new EditVocabularyModel
                                                {
                                                    Term = new Term {TermId = Constants.TERM_UpdateTermId}
                                                };
            mockView.Setup(v => v.Model).Returns(editModel);

            EditVocabularyPresenter presenter = CreatePresenter(mockView, true);

            // Act
            mockView.Raise(v => v.SaveTerm += null, EventArgs.Empty);

            // Assert
            mockView.Verify(v => v.ShowTermEditor(false));
        }

        #endregion

        #region SaveVocabulary Tests

        [Test]
        public void EditVocabularyPresenter_SaveVocabulary_Validates_Vocabulary()
        {
            // Arrange
            Mock<IEditVocabularyView> mockView = new Mock<IEditVocabularyView>();
            EditVocabularyModel editModel = new EditVocabularyModel
                                                {
                                                    Vocabulary = new Vocabulary
                                                                     {
                                                                         VocabularyId =
                                                                             Constants.VOCABULARY_UpdateVocabularyId,
                                                                         ScopeTypeId = 1
                                                                     }
                                                };
            mockView.Setup(v => v.Model).Returns(editModel);

            EditVocabularyPresenter presenter = CreatePresenter(mockView, Constants.VOCABULARY_UpdateVocabularyId, true);

            Mock<ObjectValidator> mockValidator = MockHelper.EnableValidMockValidator(presenter.Validator,
                                                                                      editModel.Vocabulary);

            // Act
            mockView.Raise(v => v.Save += null, EventArgs.Empty);

            // Assert
            mockValidator.Verify(v => v.ValidateObject(editModel.Vocabulary));
        }

        [Test]
        public void EditVocabularyPresenter_SaveVocabulary_Does_Not_Save_If_Vocabulary_Invalid()
        {
            // Arrange
            Mock<IEditVocabularyView> mockView = new Mock<IEditVocabularyView>();
            EditVocabularyModel editModel = new EditVocabularyModel
                                                {
                                                    Vocabulary = new Vocabulary
                                                                     {
                                                                         VocabularyId =
                                                                             Constants.VOCABULARY_UpdateVocabularyId,
                                                                         ScopeTypeId = 1
                                                                     }
                                                };
            mockView.Setup(v => v.Model).Returns(editModel);

            EditVocabularyPresenter presenter = CreatePresenter(mockView, Constants.VOCABULARY_UpdateVocabularyId, true);

            Mock<ObjectValidator> mockValidator = MockHelper.EnableInvalidMockValidator(presenter.Validator,
                                                                                        editModel.Vocabulary);

            // Act
            mockView.Raise(v => v.Save += null, EventArgs.Empty);

            // Assert
            Mock.Get(presenter.VocabularyController)
                .Verify(r => r.UpdateVocabulary(editModel.Vocabulary), Times.Never());
        }

        [Test]
        public void EditVocabularyPresenter_SaveVocabulary_Saves_If_Vocabulary_Valid()
        {
            // Arrange
            Mock<IEditVocabularyView> mockView = new Mock<IEditVocabularyView>();
            EditVocabularyModel editModel = new EditVocabularyModel
                                                {
                                                    Vocabulary = new Vocabulary
                                                                     {
                                                                         VocabularyId =
                                                                             Constants.VOCABULARY_UpdateVocabularyId,
                                                                         ScopeTypeId = 1
                                                                     }
                                                };
            mockView.Setup(v => v.Model).Returns(editModel);

            EditVocabularyPresenter presenter = CreatePresenter(mockView, Constants.VOCABULARY_UpdateVocabularyId, true);

            // Act
            mockView.Raise(v => v.Save += null, EventArgs.Empty);

            // Assert
            Mock.Get(presenter.VocabularyController)
                .Verify(
                    r =>
                    r.UpdateVocabulary(It.Is<Vocabulary>(v => v.VocabularyId == Constants.VOCABULARY_UpdateVocabularyId)));
        }

        [Test]
        public void EditVocabularyPresenter_SaveVocabulary_Redirects_To_Vocabulary_List_View_With_No_Errors()
        {
            // Arrange
            Mock<IEditVocabularyView> mockView = new Mock<IEditVocabularyView>();
            EditVocabularyModel editModel = new EditVocabularyModel
                                                {
                                                    Vocabulary = new Vocabulary
                                                                     {
                                                                         VocabularyId =
                                                                             Constants.VOCABULARY_UpdateVocabularyId,
                                                                         ScopeTypeId = 1
                                                                     }
                                                };
            mockView.Setup(v => v.Model).Returns(editModel);

            Mock<HttpResponseBase> mockHttpResponse = new Mock<HttpResponseBase>();

            EditVocabularyPresenter presenter = CreatePresenter(mockView, mockHttpResponse,
                                                                Constants.VOCABULARY_UpdateVocabularyId, true);
            presenter.TabId = Constants.TAB_ValidId;

            // Act
            mockView.Raise(v => v.Save += null, EventArgs.Empty);

            // Assert
            mockHttpResponse.Verify(r => r.Redirect(Globals.NavigateURL(Constants.TAB_ValidId)));
        }

        #endregion

        #region SelectedTerm Tests

        [Test]
        public void EditVocabularyPresenter_Loads_SelectedTerm_When_Term_Is_Selected()
        {
            // Arrange
            Mock<IEditVocabularyView> mockView = new Mock<IEditVocabularyView>();

            EditVocabularyModel editModel = new EditVocabularyModel
                                                {
                                                    Term = new Term {TermId = Constants.TERM_ValidTermId}
                                                };
            mockView.Setup(v => v.Model).Returns(editModel);

            EditVocabularyPresenter presenter = CreatePresenter(mockView, true);

            // Act
            mockView.Raise(v => v.SelectTerm += null, new TermsEventArgs(editModel.Term));

            // Assert
            Mock.Get(presenter.View)
                .Verify(v => v.BindTerm(editModel.Term,
                                        It.IsAny<IEnumerable<Term>>(),
                                        presenter.IsHeirarchical,
                                        false,
                                        false));
        }

        [Test]
        public void EditVocabularyPresenter_Displays_Term_Editor_When_Term_Is_Selected()
        {
            // Arrange
            Mock<IEditVocabularyView> mockView = new Mock<IEditVocabularyView>();

            EditVocabularyModel editModel = new EditVocabularyModel
                                                {
                                                    Term = new Term {TermId = Constants.TERM_ValidTermId}
                                                };
            mockView.Setup(v => v.Model).Returns(editModel);

            EditVocabularyPresenter presenter = CreatePresenter(mockView, true);

            // Act
            mockView.Raise(v => v.SelectTerm += null, new TermsEventArgs(editModel.Term));

            // Assert
            mockView.Verify(v => v.ShowTermEditor(true));
        }

        [Test]
        public void EditVocabularyPresenter_Correctly_Sets_TermEditor_Mode_When_Term_Is_Selected()
        {
            // Arrange
            Mock<IEditVocabularyView> mockView = new Mock<IEditVocabularyView>();

            EditVocabularyModel editModel = new EditVocabularyModel
                                                {
                                                    Term = new Term {TermId = Constants.TERM_ValidTermId}
                                                };
            mockView.Setup(v => v.Model).Returns(editModel);

            EditVocabularyPresenter presenter = CreatePresenter(mockView, true);

            // Act
            mockView.Raise(v => v.SelectTerm += null, new TermsEventArgs(editModel.Term));

            // Assert
            mockView.Verify(v => v.SetTermEditorMode(false, Constants.TERM_ValidTermId));
        }

        #endregion

        #region Helpers

        protected EditVocabularyPresenter CreatePresenter(Mock<IEditVocabularyView> mockView, bool initialize)
        {
            return CreatePresenter(mockView, new Mock<HttpResponseBase>(), Constants.VOCABULARY_ValidVocabularyId,
                                   initialize);
        }

        protected EditVocabularyPresenter CreatePresenter(Mock<IEditVocabularyView> mockView, int vocabularyId,
                                                          bool initialize)
        {
            return CreatePresenter(mockView, new Mock<HttpResponseBase>(), vocabularyId, initialize);
        }

        protected EditVocabularyPresenter CreatePresenter(Mock<IEditVocabularyView> mockView,
                                                          Mock<HttpResponseBase> mockHttpResponse, bool initialize)
        {
            return CreatePresenter(mockView, mockHttpResponse, Constants.VOCABULARY_ValidVocabularyId, initialize);
        }

        protected EditVocabularyPresenter CreatePresenter(Mock<IEditVocabularyView> mockView,
                                                          Mock<HttpResponseBase> mockHttpResponse, int vocabularyId,
                                                          bool initialize)
        {
            Mock<HttpRequestBase> mockHttpRequest = new Mock<HttpRequestBase>();
            NameValueCollection requestParams = new NameValueCollection();
            requestParams.Add("VocabularyId", vocabularyId.ToString());
            mockHttpRequest.Setup(r => r.Params).Returns(requestParams);

            return CreatePresenter(mockView, mockHttpResponse, mockHttpRequest, initialize);
        }

        protected EditVocabularyPresenter CreatePresenter(Mock<IEditVocabularyView> mockView,
                                                          Mock<HttpResponseBase> mockHttpResponse,
                                                          Mock<HttpRequestBase> mockHttpRequest, bool initialize)
        {
            MockHelper.CreateMockScopeTypeController();

            Mock<HttpContextBase> mockHttpContext = new Mock<HttpContextBase>();
            mockHttpContext.Setup(h => h.Response).Returns(mockHttpResponse.Object);
            mockHttpContext.Setup(h => h.Request).Returns(mockHttpRequest.Object);

            EditVocabularyPresenter presenter = new EditVocabularyPresenter(mockView.Object,
                                                                            MockHelper.CreateMockVocabularyController().
                                                                                Object,
                                                                            MockHelper.CreateMockTermController().Object)
                                                    {
                                                        HttpContext = mockHttpContext.Object
                                                    };

            if (initialize)
            {
                mockView.Raise(v => v.Initialize += null, EventArgs.Empty);
                mockView.Raise(v => v.Load += null, EventArgs.Empty);
            }

            return presenter;
        }

        #endregion
    }
}