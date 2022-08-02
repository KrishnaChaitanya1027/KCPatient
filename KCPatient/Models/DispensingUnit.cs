﻿using System;
using System.Collections.Generic;

namespace KCPatient.Models
{
    public partial class DispensingUnit
    {
        public DispensingUnit()
        {
            Medication = new HashSet<Medication>();
        }

        public string DispensingCode { get; set; }

        public virtual ICollection<Medication> Medication { get; set; }
    }
}
