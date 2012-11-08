﻿//
//  Copyright 2012  Xamarin Inc.
//
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
//
//        http://www.apache.org/licenses/LICENSE-2.0
//
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FieldService.Data {
    public class SampleAssignmentService : IAssignmentService {
        private List<Document> _documents;

        public Task<List<Assignment>> GetAssignmentsAsync ()
        {
            return Database.GetConnection ()
                .QueryAsync<Assignment> (@"
                    select Assignment.*, 
                           (SELECT SUM(Labor.Ticks) FROM Labor WHERE Assignment.ID = Labor.Assignment) as TotalTicks,       
                           (SELECT COUNT(AssignmentItem.ID) FROM AssignmentItem WHERE Assignment.ID = AssignmentItem.Assignment) AS TotalItems,       
                           (SELECT SUM(Expense.Cost) FROM Expense WHERE Assignment.ID = Expense.Assignment) AS TotalExpenses
                    from Assignment
                    where Assignment.Status != ? and Assignment.Status !=?
                    order by Assignment.Priority
                ", AssignmentStatus.Declined, AssignmentStatus.Complete);
        }

        public Task<List<Item>> GetItemsAsync ()
        {
            return Database.GetConnection ()
                .Table<Item> ()
                .OrderBy (i => i.Name)
                .ToListAsync ();
        }

        public Task<List<AssignmentItem>> GetItemsForAssignmentAsync (Assignment assignment)
        {
            return Database.GetConnection ()
                .QueryAsync<AssignmentItem> (@"
                    select AssignmentItem.*, Item.Number, Item.Name
                    from AssignmentItem
                    inner join Item
                    on Item.ID = AssignmentItem.Item
                    where AssignmentItem.Assignment = ?
                    order by Item.Name",
                    assignment.ID);
        }

        public Task<List<Labor>> GetLaborForAssignmentAsync (Assignment assignment)
        {
            return Database.GetConnection ()
                .Table<Labor> ()
                .Where (l => l.Assignment == assignment.ID)
                .ToListAsync ();
        }

        public Task<List<Expense>> GetExpensesForAssignmentAsync (Assignment assignment)
        {
            return Database.GetConnection ()
                .Table<Expense> ()
                .Where (e => e.Assignment == assignment.ID)
                .ToListAsync ();
        }

        public Task<List<Photo>> GetPhotosForAssignmentAsync (Assignment assignment)
        {
            return Database.GetConnection ()
                .Table<Photo> ()
                .Where (p => p.Assignment == assignment.ID)
                .ToListAsync ();
        }

        public Task<int> SaveAssignment (Assignment assignment)
        {
            return Database.GetConnection ()
                .UpdateAsync (assignment);
        }

        public Task<int> SaveAssignmentItem (AssignmentItem assignmentItem)
        {
            if (assignmentItem.ID == 0)
                return Database.GetConnection ().InsertAsync (assignmentItem);
            else
                return Database.GetConnection ().UpdateAsync (assignmentItem);
        }

        public Task<int> SaveLabor (Labor labor)
        {
            if (labor.ID == 0)
                return Database.GetConnection ().InsertAsync (labor);
            else
                return Database.GetConnection ().UpdateAsync (labor);
        }

        public Task<int> SaveExpense (Expense expense)
        {
            if (expense.ID == 0)
                return Database.GetConnection ().InsertAsync (expense);
            else
                return Database.GetConnection ().UpdateAsync (expense);
        }

        public Task<int> SavePhoto (Photo photo)
        {
            if (photo.ID == 0)
                return Database.GetConnection ().InsertAsync (photo);
            else
                return Database.GetConnection ().UpdateAsync (photo);
        }

        public Task<int> DeleteAssignment (Assignment assignment)
        {
            return Database.GetConnection ().DeleteAsync (assignment);
        }

        public Task<int> DeleteAssignmentItem (AssignmentItem assignmentItem)
        {
            return Database.GetConnection ().DeleteAsync (assignmentItem);
        }

        public Task<int> DeleteLabor (Labor labor)
        {
            return Database.GetConnection ().DeleteAsync (labor);
        }

        public Task<int> DeleteExpense (Expense expense)
        {
            return Database.GetConnection ().DeleteAsync (expense);
        }

        public Task<int> DeletePhoto (Photo photo)
        {
            return Database.GetConnection ().DeleteAsync (photo);
        }

        public Task<int> SaveTimerEntry (TimerEntry entry)
        {
            //If the ID is zero, it's an insert, also set the ID to 1
            if (entry.ID == 0) {
                entry.ID = 1;
                return Database.GetConnection ().InsertAsync (entry);
            } else {
                return Database.GetConnection ().UpdateAsync (entry);
            }
        }

        public Task<int> DeleteTimerEntry (TimerEntry entry)
        {
            return Database.GetConnection ().DeleteAsync (entry);
        }

        public Task<TimerEntry> GetTimerEntryAsync ()
        {
            //Just return the first row
            return Database.GetConnection ().FindAsync<TimerEntry> (_ => true);
        }

        public Task<List<Document>> GetDocumentsAsync ()
        {
            return Task.Factory.StartNew (() => {
                if (_documents == null) {
                    _documents = new List<Document> {
                        new Document { Title = "Hello, MVC", Path = "Data/Hello_MVC.pdf", Type = DocumentType.Contract },
                        new Document { Title = "Building Cross Platform Apps", Path = "Data/Building_Cross_Platform_Apps.pdf", Type = DocumentType.Contract },
                        new Document { Title = "Intro to Web Services", Path = "Data/Intro_to_Web_Services.pdf", Type = DocumentType.Contract },
                        new Document { Title = "Introduction to Mobile Development", Path = "Data/Introduction_to_Mobile_Development.pdf", Type = DocumentType.Contract },
                        new Document { Title = "Hello, iPhone", Path = "Data/Hello_iPhone.pdf", Type = DocumentType.ServiceAgreement },
                        new Document { Title = "Hello, Mono for Android", Path = "Data/Hello_Mono_for_Android.pdf", Type = DocumentType.Specifications },
                    };
                }
                return _documents;
            });
        }

        public Task<List<AssignmentHistory>> GetAssignmentHistoryAsync (Assignment assignment)
        {
            return Database.GetConnection ()
                .Table<AssignmentHistory> ()
                .Where (a => a.Assignment == assignment.ID)
                .ToListAsync ();
        }
    }
}