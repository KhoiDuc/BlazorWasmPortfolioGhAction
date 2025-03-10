﻿using System;
using System.Collections.Generic;

namespace BlazorWasmPortfolioGhAction.Pages.ConverterComponents
{
    public class OpenCalculatorInstance
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Type ComponentType { get; set; }
        public Dictionary<string, object> Parameters { get; set; }
    }
}
