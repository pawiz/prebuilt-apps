//
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
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using FieldService.Data;
using System.Text;
using FieldService.ViewModels;
using FieldService.Utilities;

namespace FieldService.Android {
    public class AssignmentsAdapter : ArrayAdapter<Assignment>, View.IOnClickListener {

        AssignmentViewModel assignmentViewModel;
        IList<Assignment> assignments;
        Assignment activeAssignment;
        int resourceId;

        public AssignmentsAdapter (Context context, int resourceId, IList<Assignment> assignments)
            : base (context, resourceId, assignments)
        {
            this.assignments = assignments;
            this.resourceId = resourceId;
            this.assignmentViewModel = ServiceContainer.Resolve<AssignmentViewModel> ();
        }
                
        public override View GetView (int position, View convertView, ViewGroup parent)
        {
            Assignment assignment = null;
            var view = convertView;
            if (view == null) {
                LayoutInflater inflator = (LayoutInflater)Context.GetSystemService (Context.LayoutInflaterService);
                view = inflator.Inflate (resourceId, null);
            }

            if (assignments.Count > position) {
                assignment = assignments [position];
            }
            if(assignment == null)
            {
                return view;   
            }

            view.SetBackgroundColor (Context.Resources.GetColor (Resource.Color.assignmentoffwhite));
            var number = view.FindViewById<TextView> (Resource.Id.assignmentItemNumber);
            var job = view.FindViewById<TextView> (Resource.Id.assignmentJob);
            var name = view.FindViewById<TextView> (Resource.Id.assignmentName);
            var phone = view.FindViewById<TextView> (Resource.Id.assignmentPhone);
            var address = view.FindViewById<TextView> (Resource.Id.assignmentAddress);
            var buttonLayout = view.FindViewById<LinearLayout> (Resource.Id.assignmentButtonLayout);
            var timerLayout = view.FindViewById<LinearLayout> (Resource.Id.assignmentTimerLayout);
            var timerlinearLayout = view.FindViewById<LinearLayout> (Resource.Id.timerLinearLayout);
            var spinner = view.FindViewById<Spinner> (Resource.Id.assignmentStatus);
            var spinnerImage = view.FindViewById<ImageView> (Resource.Id.assignmentStatusImage);
            var timer = view.FindViewById<ToggleButton> (Resource.Id.assignmentTimer);
            var timerText = view.FindViewById<TextView> (Resource.Id.assignmentTimerText);
            var accept = view.FindViewById<Button> (Resource.Id.assignmentAccept);
            var decline = view.FindViewById<Button> (Resource.Id.assignmentDecline);
            
            if (assignment.Status == AssignmentStatus.New) {
                buttonLayout.Visibility = ViewStates.Visible;
                timerLayout.Visibility = ViewStates.Gone;
            } else {
                buttonLayout.Visibility = ViewStates.Gone;
                timerLayout.Visibility = ViewStates.Visible;
                timerlinearLayout.Visibility = ViewStates.Gone;

                List<string> status = new List<string> ();
                foreach (var item in Enum.GetValues (typeof (AssignmentStatus))) {
                    status.Add (item.ToString ());
                }
                spinner.Tag = position;
                var adapter = new ArrayAdapter<string> (Context, Android.Resource.Layout.SimpleSpinnerItem, status);
                spinner.Adapter = adapter;
                spinner.ItemSelected += (sender, e) => {
                    var selected = status.ElementAtOrDefault (e.Position);
                    if (selected != null) {
                        switch (selected) {
                            case "Active":
                                view.SetBackgroundColor (Context.Resources.GetColor (Resource.Color.assignmentblue));
                                spinnerImage.SetImageResource (Resource.Drawable.EnrouteImage);
                                activeAssignment = GetItem (int.Parse (spinner.Tag.ToString ()));
                                activeAssignment.Status = AssignmentStatus.Active;
                                break;
                            default:
                                view.SetBackgroundColor (Context.Resources.GetColor (Resource.Color.assignmentgrey));
                                spinnerImage.SetImageResource (Resource.Drawable.HoldImage);
                                break;
                        }
                    }
                };
                switch (assignment.Status) {
                    case AssignmentStatus.Active:
                        break;
                    default:
                        spinner.SetSelection (status.IndexOf (assignment.Status.ToString ()));
                        break;
                }
            }

            number.Text = assignment.Priority.ToString();
            job.Text = string.Format ("#{0} {1}\n{2}", assignment.JobNumber, assignment.StartDate.ToShortDateString (), assignment.Title);
            name.Text = assignment.ContactName;
            phone.Text = assignment.ContactPhone;
            address.Text = string.Format ("{0}\n{1}, {2} {3}", assignment.Address, assignment.City, assignment.State, assignment.Zip);
            accept.SetOnClickListener (this);
            decline.SetOnClickListener (this);

            return view;
        }

        /// <summary>
        /// Activate the timer for keeping track of the active assignments time.
        /// </summary>
        /// <param name="timerChanged"></param>
        private void TimerCheckedChanged (bool timerChanged)
        {
            
        }

        /// <summary>
        /// Save assignment to the view model.
        /// </summary>
        private void SaveAssignment ()
        {
            assignmentViewModel.SaveAssignment (activeAssignment).ContinueOnUIThread (_ => {
                var activity = ServiceContainer.Resolve<AssignmentsActivity> ();
                if (activeAssignment.Status == AssignmentStatus.Active) {
                    activity.ReloadAssignments ();
                } else {

                }
            });
        }

        public void OnClick (View v)
        {
            switch (v.Id) {
                case Resource.Id.assignmentAccept:
                    activeAssignment.Status = AssignmentStatus.Active;
                    SaveAssignment ();
                    break;
                case Resource.Id.assignmentDecline:
                    activeAssignment.Status = AssignmentStatus.Declined;
                    SaveAssignment ();
                    break;
                default:
                    break;
            }
        }
    }
}