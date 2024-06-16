using System;
using System.Collections.Specialized;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms.VisualStyles;

namespace WinFormsApp1
{
    public partial class Form1 : Form
    {
        public CirclesContainer container = new CirclesContainer();
        public Form1()
        {
            InitializeComponent();
            this.KeyPreview = true;
        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            container.checkCursorAll(e.X, e.Y, checkBox1.Checked, checkBox2.Checked);
            pictureBox1.Invalidate();
        }

        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {

            container.paintAllCircles(e.Graphics);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            ActiveControl = checkBox2;
        }
        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                container.delete_objects();
                pictureBox1.Invalidate();
            }
            else if (e.KeyCode == Keys.ControlKey)
            {
                checkBox2.Checked = true;
            }

        }

        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.ControlKey)
            {
                checkBox2.Checked = false;
            }
        }
    }


    public class Strategy
    {
        public virtual void push_back(ref CCircle[] arr, ref int current_size, CCircle obj) {}
    }
    public class StrategyPushBackDefault: Strategy
    {
        public override void push_back(ref CCircle[] arr, ref int current_size, CCircle obj) {
            arr[current_size++] = obj;
        }

    }

    public class StrategyPushBackLimit: Strategy
    {
        public override void push_back(ref CCircle[] arr, ref int current_size, CCircle obj)
        {
            CCircle[] new_container = new CCircle[current_size + 10];
            for (int i = 0; i < current_size; i++)
                new_container[i] = arr[i];

            new_container[current_size++] = obj;
            arr = new_container;
        }
    }

    public class CCircle
    {
        private int x;
        private int y;
        private int radius;
        private bool isSelected;
        
        public CCircle(int x, int y, int radius)
        {
            this.x = x;
            this.y = y;
            this.radius = radius;
            this.isSelected = false;
        }

        public void paintCircle(Graphics g)
        {
            Rectangle rect = new Rectangle(x - radius, y - radius, 2 * radius, 2 * radius);
            if (isSelected)
                g.FillEllipse(Brushes.Blue, rect);
            else
                g.FillEllipse(Brushes.Red, rect);
        }

        
        public bool checkCursor(int mouseX, int mouseY)
        {
            int dx = mouseX - x;
            int dy = mouseY - y;
            return dx * dx + dy * dy <= radius * radius;
        }

        public void select()
        {
            isSelected = true;
        }

        public void unselect()
        {
            isSelected=false;
        }

        public bool selected()
        {
            return isSelected;
        }
    }



    public class CirclesContainer
    {
        private int capacity;
        private CCircle[] container;
        private int current_size = 0;
        private Strategy strategy = new StrategyPushBackDefault();
        
        public CirclesContainer()
        {
            this.capacity = 10;
            container = new CCircle[this.capacity];
            
        }

        public CirclesContainer(int capacity)
        {
            this.capacity = capacity;
            container = new CCircle[this.capacity];
        }

        public void push_back(CCircle obj)
        {
            if (current_size >= capacity)
            {
                strategy = new StrategyPushBackLimit();
                capacity += 10;
                strategy.push_back(ref container, ref current_size, obj);
                
            }
            else
                strategy.push_back(ref container, ref current_size, obj);
        }

        public void paintAllCircles(Graphics g)
        {
            for (int i = 0; i < current_size; i++)
                container[i].paintCircle(g);
        }

        
        public void checkCursorAll(int mouseX, int mouseY, bool checkBox1State, bool checkBox2State)
        {
            List<int> indexes = cursorOnCircle(mouseX, mouseY);
            if (indexes.Count == 0)
            {
                unselectall();
                push_back(new CCircle(mouseX, mouseY, 30));
                container[current_size - 1].select();
            }
            else if (indexes.All(i => container[i].selected()) && get_selected_circles_count()>indexes.Count)
            {
                for (int i = 0; i < indexes.Count; i++)
                    container[indexes[i]].unselect();
            }
            
            else if (!checkBox1State && !checkBox2State)
            {
                
                for (int i = 0; i< indexes.Count; i++)
                {
                    unselectall();
                    container[indexes[i]].select();
                }
            }
            else if (!checkBox1State && checkBox2State)
            {
                
                for (int i = 0; i < indexes.Count; i++)
                {
                    container[indexes[i]].select();
                    break;
                }
            }
            else if (checkBox1State && !checkBox2State)
            {
                unselectall();
                for (int i = 0; i < indexes.Count; i++)
                {
                    container[indexes[i]].select();
                }
            }
            else
            {
                for (int i = 0; i < indexes.Count; i++)
                {
                    container[indexes[i]].select();
                }
            }
        }
        
        public int get_selected_circles_count()
        {
            int count = 0;
            for (int i = 0; i < current_size; i++)
            {
                if (container[i].selected())
                    count++;

            }
            return count;
        }

        public void delete_objects()
        {
            CCircle[] new_container = new CCircle[capacity];
            int new_container_index = 0;
            for (int i = 0; i < current_size; i++)
            {
                if (!container[i].selected())
                {
                    new_container[new_container_index++] = container[i];
                }
            }
            current_size = new_container_index;
            if (current_size!=0)
                new_container[current_size-1].select();
            container = new_container;
        }

        public List<int> cursorOnCircle(int mouseX, int mouseY)
        {
            List<int> indexes = new List<int>();
            for (int i = 0; i < current_size; i++)
            {
                if (container[i].checkCursor(mouseX, mouseY))
                    indexes.Add(i);
            }

            return indexes;
        }

        public void unselectall()
        {
            for (int i = 0; i < current_size; i++)
            {
                container[i].unselect();
                
            }
        }

        public int get_size()
        {
            return current_size;
        }

        public CCircle getCircle(int ind)
        {
            return container[ind];
        }
    }

}
