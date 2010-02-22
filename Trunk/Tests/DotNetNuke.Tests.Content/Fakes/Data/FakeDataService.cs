﻿/*
' DotNetNuke® - http://www.dotnetnuke.com
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
*/

using System;

using System.Data;
using DotNetNuke.Entities.Content;
using DotNetNuke.Entities.Content.Data;
using DotNetNuke.Entities.Content.Taxonomy;
using DotNetNuke.Tests.Utilities;
using DotNetNuke.Common.Utilities;
using DotNetNuke.ComponentModel;

namespace DotNetNuke.Tests.Content.Fakes.Data
{
    public class FakeDataService : IDataService
    {
        #region Private Members

        private static DataTable contentItemTable;
        private static DataTable contentTypeTable;
        private static DataTable scopeTypeTable;
        private static DataTable termTable;
        private static DataTable vocabularyTable;

        #endregion

        public static FakeDataService CreateFakeDataService()
        {
            if (ComponentFactory.Container == null)
                //Create a Container
                ComponentFactory.Container = new SimpleContainer();

            //Try and get dataservice
            var dataService = ComponentFactory.GetComponent<FakeDataService>();

            if (dataService == null)
            {
                // Create the data service
                dataService = new FakeDataService();

                // Add to Container
                ComponentFactory.RegisterComponentInstance<FakeDataService>(dataService);
            }

            return dataService;
        }

        #region Content Members

        public int AddContentItem(ContentItem contentItem, int createdByUserId)
        {
            return Constants.CONTENT_AddContentItemId;
        }

        public void DeleteContentItem(ContentItem contentItem)
        {
            DataRow[] row = contentItemTable.Select("ContentItemID = '" + contentItem.ContentItemId + "'");
            
            //Remove the row from the table
            if (row.Length > 0)
            {
                contentItemTable.Rows.Remove(row[0]);
            }
        }

        public IDataReader GetContentItem(int contentItemId)
        {
            DataTable tempTable = ContentTestHelper.CopyContentItemRowsToNewTable(contentItemTable.Select("ContentItemID = '" + contentItemId + "'"));
            return tempTable.CreateDataReader();
        }

        public IDataReader GetContentItemsByModule(int moduleId)
        {
            DataTable tempTable = ContentTestHelper.CopyContentItemRowsToNewTable(contentItemTable.Select("ModuleID = '" + moduleId + "'"));
            return tempTable.CreateDataReader();
        }

        public IDataReader GetUnIndexedContentItems()
        {
            DataTable tempTable = ContentTestHelper.CopyContentItemRowsToNewTable(contentItemTable.Select("Indexed = 0"));
            return tempTable.CreateDataReader();
        }

        public void UpdateContentItem(ContentItem contentItem, int lastModifiedByUserId)
        {
            DataRow[] row = contentItemTable.Select("ContentItemID = '" + contentItem.ContentItemId + "'");
            if (row.Length > 0)
            {
                row[0]["Content"] = contentItem.Content;
                row[0]["ContentKey"] = contentItem.ContentKey;
                row[0]["Indexed"] = contentItem.Indexed;
            }
        }

        #endregion

        #region ContentType Members

        public int AddContentType(ContentType contentType)
        {
            return Constants.CONTENTTYPE_AddContentTypeId;
        }

        public void DeleteContentType(ContentType contentType)
        {
            DataRow[] row = contentTypeTable.Select("ContentTypeID = '" + contentType.ContentTypeId + "'");

            //Remove the row from the table
            if (row.Length > 0)
            {
                contentTypeTable.Rows.Remove(row[0]);
            }
        }

        public IDataReader GetContentTypes()
        {
            return contentTypeTable.CreateDataReader();
        }

        public void UpdateContentType(ContentType contentType)
        {
            DataRow[] row = contentTypeTable.Select("ContentTypeID = '" + contentType.ContentTypeId + "'");
            if (row.Length > 0)
            {
                row[0]["ContentType"] = contentType.ContentType;
            }
        }

        #endregion

        #region ScopeType Members

        public int AddScopeType(ScopeType scopeType)
        {
            return Constants.SCOPETYPE_AddScopeTypeId;
        }

        public void DeleteScopeType(ScopeType scopeType)
        {
            DataRow[] row = scopeTypeTable.Select("ScopeTypeID = '" + scopeType.ScopeTypeId + "'");

            //Remove the row from the table
            if (row.Length > 0)
            {
                scopeTypeTable.Rows.Remove(row[0]);
            }
        }

        public IDataReader GetScopeTypes()
        {
            return scopeTypeTable.CreateDataReader();
        }

        public void UpdateScopeType(ScopeType scopeType)
        {
            DataRow[] row = scopeTypeTable.Select("ScopeTypeID = '" + scopeType.ScopeTypeId + "'");
            if (row.Length > 0)
            {
                row[0]["ScopeType"] = scopeType.ScopeType;
            }
        }

        #endregion

        #region Term Members

        public int AddHeirarchicalTerm(Term term, int createdByUserId)
        {
            return Constants.TERM_AddTermId;
        }

        public int AddSimpleTerm(Term term, int createdByUserId)
        {
            return Constants.TERM_AddTermId;
        }

        public void AddTermToContent(Term term, ContentItem content)
        {
        }

        public void DeleteHeirarchicalTerm(Term term)
        {
            DataRow[] row = termTable.Select("TermID = " + term.TermId.ToString());

            //Remove the row from the table
            if (row.Length > 0)
            {
                termTable.Rows.Remove(row[0]);
            }
        }

        public void DeleteSimpleTerm(Term term)
        {
            DataRow[] row = termTable.Select("TermID = " + term.TermId.ToString());

            //Remove the row from the table
            if (row.Length > 0)
            {
                termTable.Rows.Remove(row[0]);
            }
        }

        public IDataReader GetTerm(int termId)
        {
            DataTable tempTable = ContentTestHelper.CopyTermRowsToNewTable(termTable.Select("TermID = " + termId.ToString()));
            return tempTable.CreateDataReader();
        }

        public IDataReader GetTermsByContent(int contentItemId)
        {
            DataTable tempTable = ContentTestHelper.CopyTermRowsToNewTable(termTable.Select("ContentItemID = " + contentItemId.ToString()));
            return tempTable.CreateDataReader();
        }

        public IDataReader GetTermsByVocabulary(int vocabularyId)
        {
            DataTable tempTable = ContentTestHelper.CopyTermRowsToNewTable(termTable.Select("VocabularyID = " + vocabularyId.ToString()));
            return tempTable.CreateDataReader();
        }

        public void RemoveTermsFromContent(ContentItem content)
        {
        }

        public void UpdateHeirarchicalTerm(Term term, int lastModifiedByUserId)
        {
            DataRow[] row = termTable.Select("TermID = " + term.TermId.ToString());
            if (row.Length > 0)
            {
                row[0]["Name"] = term.Name;
                row[0]["Weight"] = term.Weight;
            }
        }

        public void UpdateSimpleTerm(Term term, int lastModifiedByUserId)
        {
            DataRow[] row = termTable.Select("TermID = " + term.TermId.ToString());
            if (row.Length > 0)
            {
                row[0]["Name"] = term.Name;
                row[0]["Weight"] = term.Weight;
            }
        }

        #endregion

        #region Vocabulary Members

        public int AddVocabulary(Vocabulary vocabulary, int createdByUserId)
        {
            return Constants.VOCABULARY_AddVocabularyId;
        }

        public void DeleteVocabulary(Vocabulary vocabulary)
        {
            DataRow[] row = vocabularyTable.Select("VocabularyID = " + vocabulary.VocabularyId.ToString());

            //Remove the row from the table
            if (row.Length > 0)
            {
                vocabularyTable.Rows.Remove(row[0]);
            }
        }

        public IDataReader GetVocabularies()
        {
            return vocabularyTable.CreateDataReader();
        }

        public void UpdateVocabulary(Vocabulary vocabulary, int lastModifiedByUserId)
        {
            DataRow[] row = vocabularyTable.Select("VocabularyID = " + vocabulary.VocabularyId.ToString());
            if (row.Length > 0)
            {
                row[0]["Name"] = vocabulary.Name;
                row[0]["Weight"] = vocabulary.Weight;
            }
        }

        #endregion

        #region Public Methods

        public void AddContentItemsToTable(int count, bool indexed, int startUserId)
        {
            for (int i = Constants.CONTENT_ValidContentItemId;
                       i < Constants.CONTENT_ValidContentItemId + count;
                       i++)
            {
                string content = (count == 1) ? Constants.CONTENT_ValidContent : ContentTestHelper.GetContent(i);
                string contentKey = (count == 1) ? Constants.CONTENT_ValidContentKey : ContentTestHelper.GetContentKey(i);
                int userId = (startUserId == Null.NullInteger) ? Constants.USER_ValidId + i : startUserId;

                ContentTestHelper.AddContentItemToTable(contentItemTable, i, content, contentKey, indexed, startUserId);
            }
        }

        public void AddContentTypesToTable(int count)
        {
            for (int i = Constants.CONTENTTYPE_ValidContentTypeId;
                       i < Constants.CONTENTTYPE_ValidContentTypeId + count;
                       i++)
            {
                string contentType = (count == 1) ? Constants.CONTENTTYPE_ValidContentType : ContentTestHelper.GetContentType(i);

                ContentTestHelper.AddContentTypeToTable(contentTypeTable, i, contentType);
            }
        }

        public void AddScopeTypesToTable(int count)
        {
            for (int i = Constants.SCOPETYPE_ValidScopeTypeId;
                       i < Constants.SCOPETYPE_ValidScopeTypeId + count;
                       i++)
            {
                string scopeType = (count == 1) ? Constants.SCOPETYPE_ValidScopeType : ContentTestHelper.GetScopeType(i);

                ContentTestHelper.AddScopeTypeToTable(scopeTypeTable, i, scopeType);
            }
        }

        public void AddTermsToTable(int count, Func<int, int> contentIdFunction, Func<int, int> vocabularyIdFunction)
        {
            for (int i = Constants.TERM_ValidTermId;
                       i < Constants.TERM_ValidTermId + count;
                       i++)
            {
                string name = (count == 1) ? Constants.TERM_ValidName : ContentTestHelper.GetTermName(i);
                string description = (count == 1) ? Constants.VOCABULARY_ValidName : ContentTestHelper.GetTermName(i);
                int weight = Constants.TERM_ValidWeight;
                int parentId = Constants.TERM_ValidParentTermId;
                ContentTestHelper.AddTermToTable(termTable, i, contentIdFunction(i), vocabularyIdFunction(i), name, description, weight, parentId);
            }
        }

        public void AddVocabulariesToTable(int count, int scopeTypeId, Func<int, int> scopeIdFunction, Func<int, int> typeIdFunction)
        {
            for (int i = Constants.VOCABULARY_ValidVocabularyId;
                       i < Constants.VOCABULARY_ValidVocabularyId + count;
                       i++)
            {
                string name = (count == 1) ? Constants.VOCABULARY_ValidName : ContentTestHelper.GetVocabularyName(i);
                int typeId = typeIdFunction(i);
                string description = (count == 1) ? Constants.VOCABULARY_ValidName : ContentTestHelper.GetVocabularyName(i);
                int weight = Constants.VOCABULARY_ValidWeight;
                ContentTestHelper.AddVocabularyToTable(vocabularyTable, i, typeId, name, description, scopeIdFunction(i), scopeTypeId, weight);
            }
        }

        public ContentItem GetContentItemFromTable(int contentItemId)
        {
            ContentItem content = null;
            DataRow[] row = contentItemTable.Select("ContentItemID = '" + contentItemId + "'");
            if (row.Length > 0)
            {
                content = new ContentItem();
                content.ContentItemId = contentItemId;
                content.Content = (string)row[0]["Content"];
                content.ContentKey = (string)row[0]["ContentKey"];
                content.Indexed = (bool)row[0]["Indexed"];
            }

            return content;
        }

        public ContentType GetContentTypeFromTable(int contentTypeID)
        {
            ContentType contentType = null;
            DataRow[] row = contentTypeTable.Select("ContentTypeID = '" + contentTypeID + "'");
            if (row.Length > 0)
            {
                contentType = new ContentType();
                contentType.ContentTypeId = contentTypeID;
                contentType.ContentType = (string)row[0]["ContentType"];
            }

            return contentType;
        }

        public ScopeType GetScopeTypeFromTable(int scopeTypeID)
        {
            ScopeType scopeType = null;
            DataRow[] row = scopeTypeTable.Select("ScopeTypeID = '" + scopeTypeID + "'");
            if (row.Length > 0)
            {
                scopeType = new ScopeType();
                scopeType.ScopeTypeId = scopeTypeID;
                scopeType.ScopeType = (string)row[0]["ScopeType"];
            }

            return scopeType;
        }

        public Term GetTermFromTable(int termId)
        {
            Term term = null;
            DataRow[] row = termTable.Select("TermID = " + termId.ToString());
            if (row.Length > 0)
            {
                term = new Term((int)row[0]["VocabularyID"]);
                term.TermId = (int)row[0]["TermID"];
                term.Name = (string)row[0]["Name"];
                term.Description = (string)row[0]["Description"];
                term.Weight = (int)row[0]["Weight"];
            }

            return term;
        }

        public Vocabulary GetVocabularyFromTable(int vocabularyId)
        {
            Vocabulary vocabulary = null;
            DataRow[] row = vocabularyTable.Select("VocabularyID = '" + vocabularyId + "'");
            if (row.Length > 0)
            {
                vocabulary = new Vocabulary();
                vocabulary.VocabularyId = vocabularyId;
                vocabulary.Name = (string)row[0]["Name"];
                vocabulary.Type = ((int)row[0]["VocabularyTypeID"] == 1) ? VocabularyType.Simple : VocabularyType.Hierarchy;
                vocabulary.Description = (string)row[0]["Description"];
                vocabulary.ScopeId = (int)row[0]["ScopeID"];
                vocabulary.ScopeTypeId = (int)row[0]["ScopeTypeID"];
                vocabulary.Weight = (int)row[0]["Weight"];
            }

            return vocabulary;
        }

        public void SetUpContentItemTable()
        {
            contentItemTable = ContentTestHelper.CreateContentItemTable();
        }

        public void SetUpContentTypeTable()
        {
            contentTypeTable = ContentTestHelper.CreateContentTypeTable();
        }

        public void SetUpScopeTypeTable()
        {
            scopeTypeTable = ContentTestHelper.CreateScopeTypeTable();
        }

        public void SetUpTermTable()
        {
            termTable = ContentTestHelper.CreateTermTable();
        }

        public void SetUpVocabularyTable()
        {
            vocabularyTable = ContentTestHelper.CreateVocabularyTable();
        }

        #endregion
    }

}
