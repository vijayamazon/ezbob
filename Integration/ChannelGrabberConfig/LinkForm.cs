﻿using System;
using System.Collections.Generic;

namespace Integration.ChannelGrabberConfig {
	#region class LinkForm

	public class LinkForm : ICloneable {
		#region public

		#region constructor

		public LinkForm() {
			Fields = new List<FieldInfo>();
			Notes = new List<string>();
			OnBeforeLink = new List<string>();
			SourceLabel = new List<Stylesheet>();
			SourceLabelOn = new List<Stylesheet>();
		} // constructor

		#endregion constructor

		#region properties

		public List<FieldInfo> Fields { get; set; }
		public List<string> Notes { get; set; }
		public List<string> OnBeforeLink { get; set; }
		public List<Stylesheet> SourceLabel { get; set; }
		public List<Stylesheet> SourceLabelOn { get; set; }

		#endregion properties

		#region method Validate

		public void Validate() {
			Fields = Fields ?? new List<FieldInfo>();
			Notes = Notes ?? new List<string>();
			OnBeforeLink = OnBeforeLink ?? new List<string>();
			SourceLabel = SourceLabel ?? new List<Stylesheet>();
			SourceLabelOn = SourceLabelOn ?? new List<Stylesheet>();

			if (Fields.Count < 1)
				throw new ConfigException("Fields not specified.");

			foreach (FieldInfo fi in Fields)
				fi.Validate();

			SourceLabel.ForEach( css => css.Validate() );
			SourceLabelOn.ForEach( css => css.Validate() );
		} // Validate

		#endregion method Validate

		#region method ToString

		public override string ToString() {
			return Fields.ToString();
		} // ToString

		#endregion method ToString

		#region method Clone

		public object Clone() {
			var oRes = new LinkForm();

			Fields.ForEach( fi => oRes.Fields.Add((FieldInfo)fi.Clone()) );

			Notes.ForEach( s => oRes.Notes.Add((string)s.Clone()) );

			OnBeforeLink.ForEach( s => oRes.OnBeforeLink.Add((string)s.Clone()) );

			SourceLabel.ForEach( css => oRes.SourceLabel.Add((Stylesheet)css.Clone()) );

			SourceLabelOn.ForEach( css => oRes.SourceLabelOn.Add((Stylesheet)css.Clone()) );

			return oRes;
		} // Clone

		#endregion method Clone

		#endregion public
	} // class LinkForm

	#endregion class LinkForm
} // namespace Integration.ChannelGrabberConfig
