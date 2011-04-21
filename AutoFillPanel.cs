using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.Windows.Forms;
using System.Reflection;
using System.Drawing;

namespace UtilityCodeAsset.UserInterface

{
    public class AutoFillPanel : Panel
    {
        private Type _datasource;
        //private FieldInfo[] _targets;
        private const string Prefix = "AutoFill_";
        //***** BEGIN OF INSERTION 72.6.0 - KISHAN RAMINENI
        private const string Suffix = "_JobID";
        //***** END OF INSERTION 72.6.0 - KISHAN RAMINENI
        private string[] Delim = new string[] {"_delim_"};

        public AutoFillPanel(Type datasource) : base()
        {
            _datasource = datasource;
            //ArrayList rc = new ArrayList();
            //FieldInfo[] alltargets = this.GetType().GetFields();
            //PropertyInfo[] allsources = _datasource.GetProperties();
            //PropertyInfo p;
            //for (int i = 0; i < alltargets.Length; i++)
            //{
            //    if (alltargets[i].Name.StartsWith(Prefix))
            //    {
            //        rc.Add(alltargets[i]);
            //    }
            //}
            //_targets =(FieldInfo[]) rc.ToArray(typeof(FieldInfo));
        }

        public virtual void Populate( object source )
        {
            if (source.GetType() != _datasource)
               return;
            string tmp;
            for (int i = 0; i <Controls.Count; i++)
            {
                if (!Controls[i].Name.StartsWith(Prefix))
                    continue;
                tmp = GetValueFromSource(Controls[i].Name, source);
                Controls[i].Text = tmp;
            }
        }

        private string GetValueFromSource(string targetname, object source)
        {
            try
            {
                string targetTrim = targetname.Substring(targetname.IndexOf(Delim[0]) + Delim[0].Length);
                string[] fieldname = targetTrim.Split(Delim, StringSplitOptions.RemoveEmptyEntries);
                PropertyInfo prop;
                Object rc;

                prop = source.GetType().GetProperty(fieldname[0]);
                if (prop == null)
                    return "";
                // See if it is an ref object within the source, instead of a primitive type
                rc = prop.GetValue(source, null);
                if (fieldname.Length > 1)
                {
                    return GetValueFromSource(targetTrim, rc);
                }
                //if (prop.PropertyType.Name == "Double")
                //{
                //    double doublerc = (double)rc;
                //    return doublerc.ToString();
                //}
                return rc.ToString();
            }
            catch (Exception e)
            {
                return "";
            }
        }


        /// <summary>
        /// This method provides the width of a given text, with the given font
        /// 
        /// </summary>
        /// <param name="text"></param>
        /// <param name="size"></param>
        /// <returns>Text width</returns>
        protected int GetTextWidth(string text, Control control)
        {
            float textwidth=0;
            Graphics g = control.CreateGraphics();
            textwidth = (int)g.MeasureString(text, control.Font).Width;
            g.Dispose();
            return (int)Math.Ceiling(textwidth);
        }


        /// <summary>
        /// Get the window control based on the provided name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        protected virtual Control GetControl(string name)
        {
            Control[] c_list = this.Controls.Find(name, true);
            return (c_list != null && c_list.Length > 0) ? c_list[0] : null;
        }
    }
}
