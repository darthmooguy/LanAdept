﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LanAdeptData.Model
{
	public class Gamer
	{
		public int GamerID { get; set; }

		public string Gamertag { get; set; }

		public virtual int UserID { get; set; }

		#region Navigation properties
		[ForeignKey("UserID")]
		public virtual User User { get; set; }
		#endregion
	}
}