﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Loyc
{
	public interface ICloneable<out T>
	{
		T Clone();
	}
}
