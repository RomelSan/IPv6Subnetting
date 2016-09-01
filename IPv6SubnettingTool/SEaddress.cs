﻿/*
 * Copyright (c) 2010-2016 Yucel Guven
 * All rights reserved.
 * 
 * This file is part of IPv6 Subnetting Tool.
 * 
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted (subject to the limitations in the
 * disclaimer below) provided that the following conditions are met:
 * 
 * 1. Redistributions of source code must retain the above copyright
 * notice, this list of conditions and the following disclaimer.
 * 2. Redistributions in binary form must reproduce the above copyright
 * notice, this list of conditions and the following disclaimer in the
 * documentation and/or other materials provided with the distribution.
 * 
 * NO EXPRESS OR IMPLIED LICENSES TO ANY PARTY'S PATENT RIGHTS ARE
 * GRANTED BY THIS LICENSE. THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS 
 * AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, 
 * BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS 
 * FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER
 * OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY,
 * OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
 * SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
 * INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT,
 * STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT
 * OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

using System.Numerics;
using System.Collections.Generic;

namespace IPv6SubnettingTool
{
    public class SEaddress
    {
        public BigInteger Resultv6 = BigInteger.Zero;
        public BigInteger LowerLimitAddress = BigInteger.Zero;
        public BigInteger Start = BigInteger.Zero;
        public BigInteger End = BigInteger.Zero;
        public BigInteger UpperLimitAddress = BigInteger.Zero;
        public int slash = 0;
        public int subnetslash = 0;
        public BigInteger subnetidx = BigInteger.Zero;
        public int upto = 0;
        public List<string> liste = new List<string>();
        public int ID = 0;
        
        // Initialize all values - default or MinValue
        public void Initialize()
        {
            this.Resultv6 = BigInteger.Zero;
            this.LowerLimitAddress = BigInteger.Zero;
            this.Start = BigInteger.Zero;
            this.End = BigInteger.Zero;
            this.UpperLimitAddress = BigInteger.Zero;
            this.slash = 0;
            this.subnetslash = 0;
            this.subnetidx = BigInteger.Zero;
            this.upto = 0;
            this.ID = 0;
            this.liste.Clear();
        }
    }
}
