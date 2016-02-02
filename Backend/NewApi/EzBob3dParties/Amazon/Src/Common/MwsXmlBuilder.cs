﻿/******************************************************************************* 
 * Copyright 2009-2012 Amazon Services. All Rights Reserved.
 * Licensed under the Apache License, Version 2.0 (the "License"); 
 *
 * You may not use this file except in compliance with the License. 
 * You may obtain a copy of the License at: http://aws.amazon.com/apache2.0
 * This file is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
 * CONDITIONS OF ANY KIND, either express or implied. See the License for the 
 * specific language governing permissions and limitations under the License.
 *******************************************************************************
 * Marketplace Web Service Runtime Client Library
 */

namespace EzBob3dParties.Amazon.Src.Common
{
    using System.Text;
    using System.Xml;

    public class MwsXmlBuilder : MwsXmlWriter
    {
        StringBuilder builder;
        public MwsXmlBuilder(bool toWrap)
        {            
            this.builder = new StringBuilder();
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.OmitXmlDeclaration = !toWrap;
            writer = XmlWriter.Create(this.builder, settings);
        }


        public MwsXmlBuilder() : this(false) { }

        public MwsXmlBuilder(bool toWrap, ConformanceLevel conformanceLevel)
        {
            this.builder = new StringBuilder();
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.OmitXmlDeclaration = !toWrap;
            settings.ConformanceLevel = conformanceLevel;
            writer = XmlWriter.Create(this.builder, settings);
        }

        public override string ToString()
        {
            writer.Flush();
            return this.builder.ToString(0, this.builder.Length);
        }

    }
}
