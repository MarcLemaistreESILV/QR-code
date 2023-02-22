using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nom
{
    public class Pixel
    {
        private byte r;
        private byte g;
        private byte b;

        public Pixel(byte r, byte g, byte b)
        {
            this.r = r;
            this.g = g;
            this.b = b;

        }
        public byte R
        {
            get { return this.r; }
            set { this.r = value; }
        }
        public byte G
        {
            get { return this.g; }
            set { this.g = value; }
        }
        public byte B
        {
            get { return this.b; }
            set { this.b = value; }
        }
        public bool est_noir()
        {
            if (this.r == 0 && this.g == 0 && this.b == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public string ToString()
        {
            return this.r + "" + this.g + "" + this.b + "";
        }
        //new
        public void equal_pixel_of(Pixel equalize)
        {
            this.r = equalize.R;
            this.g = equalize.G;
            this.b = equalize.B;
        }
        public string ToString_noir()
        {
            if (this.r == 0)
            {
                return "0";
            }
            else
            {
                return "1";
            }

        }
    }
}
