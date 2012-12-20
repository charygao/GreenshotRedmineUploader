﻿/*
 * Created by SharpDevelop.
 * User: Michael Kling - Ascendro S.R.L
 * Date: 19.12.2012
 * Time: 3:33 
 * 
 *  Copyright Michael Kling - Ascendro S.R.L
 *
 *  Licensed under the Apache License, Version 2.0 (the "License");
 *  you may not use this file except in compliance with the License.
 *  You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 *  Unless required by applicable law or agreed to in writing, software
 *  distributed under the License is distributed on an "AS IS" BASIS,
 *  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 *  See the License for the specific language governing permissions and
 *  limitations under the License.
 */
using System;
using Redmine.Net.Api;
using Redmine.Net.Api.Types;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Windows.Forms;
using System.Collections;
using System.IO;

namespace GreenshotRedmineUploader
{
	public class RedmineSettings 
	{				
		public RedmineDataBuffer buffer;
		
		private RedmineManager manager;
		public RedmineSettings()
		{
			buffer = RedmineDataBuffer.Read();
			manager = new RedmineManager(buffer.host, buffer.apikey);
		}
				
		public bool syncWithRed() {
			buffer.projects.Clear();
			buffer.trackers.Clear();
			buffer.priorities.Clear();
			buffer.statuses.Clear();
			buffer.trackers.Clear();
			
			try {
				var parameters = new NameValueCollection {{"include", "memberships"}};
				var currentUser = manager.GetCurrentUser(parameters);				
				foreach (var membership in currentUser.Memberships) {
					buffer.projects.Add(membership.Project.Name,membership.Project.Id);            								
				}		
				
				
				parameters = new NameValueCollection {};
	            foreach (var tracker in manager.GetObjectList<Tracker>(parameters))
	            {
	            	buffer.trackers.Add(tracker.Name,tracker.Id);
	            }
	            
	            //Maybe in the future we can request the enums.            
	            buffer.priorities.Add("Very low",5);
	            buffer.priorities.Add("Low",1);
	            buffer.priorities.Add("Normal",2);
	            buffer.priorities.Add("High",3);
	            buffer.priorities.Add("Critical",4);
	            buffer.priorities.Add("Default",0);
	                      
				parameters = new NameValueCollection {};
	            foreach (var status in manager.GetObjectList<IssueStatus>(parameters))
	            {
	            	buffer.statuses.Add(status.Name,status.Id);            	
	            }
	            buffer.statuses.Add("Default",0);
			} catch (Redmine.Net.Api.RedmineException e) {
				MessageBox.Show(e.Message,"Error while Syncing.");
				buffer.projects.Clear();
				buffer.trackers.Clear();
				buffer.priorities.Clear();
				buffer.statuses.Clear();
				buffer.trackers.Clear();
				return false;
			}             
			return true;
		}
		
		public Hashtable getIssueAssigneeList(string issueId) {	
			var parameters = new NameValueCollection {};
			try {
		 		var issue = manager.GetObject<Issue>(issueId,parameters);
		 		return getProjectAssigneeList(issue.Project.Id.ToString());
			} catch (Redmine.Net.Api.RedmineException e) {
				MessageBox.Show(e.Message,"Error while checking issue.");
			}
			return new Hashtable();
		}
		
		public Hashtable getProjectAssigneeList(string projectId) {
			Hashtable assigneeList = new Hashtable();
			try {
				var parameters = new NameValueCollection {{"project_id", projectId}};
				foreach (var member in manager.GetObjectList<ProjectMembership>(parameters))
	            {
					if (member.User != null) {
						assigneeList.Add(member.User.Name,member.User.Id);
					} else {
						assigneeList.Add(member.Group.Name,member.Group.Id);
					}
	            }
			} catch (Redmine.Net.Api.RedmineException e) {
				MessageBox.Show(e.Message,"Error while checking issue.");
			}
			return assigneeList;
			
		}
		
		public Upload uploadFile(string filename) {
			//Upload data is not attaching any authorisation keys... so we need to implement it by ourself.
			return manager.UploadData(FileToByteArray(filename));
		}
		
		public Issue getIssue(string Id) {
			var parameters = new NameValueCollection {{"include","journals,changesets"}};
			return manager.GetObject<Issue>(Id,parameters);
		}
		
		public Issue createIssue(Issue issue) {
			return manager.CreateObject<Issue>(issue);
		}
		
		public Journal createJournal(Journal journal,Issue issue) {
			return manager.CreateObject<Journal>(journal);
		}
		   
		public void updateIssue(Issue issue) {
			manager.UpdateObject<Issue>(issue.Id.ToString(),issue);
		}
		
        /// <summary>
		/// Function to get byte array from a file
		/// </summary>
		/// <param name="_FileName">File name to get byte array</param>
		/// <returns>Byte Array</returns>
		private byte[] FileToByteArray(string _FileName)
		{
		    byte[] _Buffer = null;
		
	        // Open file for reading
	        System.IO.FileStream _FileStream = new System.IO.FileStream(_FileName, System.IO.FileMode.Open, System.IO.FileAccess.Read);
	        
	        // attach filestream to binary reader
	        System.IO.BinaryReader _BinaryReader = new System.IO.BinaryReader(_FileStream);
	        
	        // get total byte length of the file
	        long _TotalBytes = new System.IO.FileInfo(_FileName).Length;
	        
	        // read entire file into buffer
	        _Buffer = _BinaryReader.ReadBytes((Int32)_TotalBytes);
	        
	        // close file reader
	        _FileStream.Close();
	        _FileStream.Dispose();
	        _BinaryReader.Close();
		
		    return _Buffer;
		}
		
	}
}