using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows;

namespace Stroke
{
    public static class Mouse
    {
        private static readonly double ratioX, ratioY;  //鼠标移动的坐标需要系数缩放
        private static Point savedPoint=new Point(-1,-1);
        static Mouse()
        {
            Graphics graphics = Graphics.FromHwnd(IntPtr.Zero);
            double width = SystemParameters.VirtualScreenWidth * graphics.DpiX / 96;
            double height = SystemParameters.VirtualScreenHeight * graphics.DpiY / 96;
            //缩放系数=65536/屏幕分辨率（sendinput中的x，y是0到65535之间的数，映射到整个屏幕）
            //屏幕分辨率=屏幕像素点*dpi/96
            ratioX = 65536 / width;
            ratioY = 65536 / height;
        }

        //处理坐标缩放
        private static void HandleCoordinates(ref Point point) 
        {

            point.X = (int)Math.Round(point.X * ratioX, 0);
            point.Y = (int)Math.Round(point.Y * ratioY, 0);
            return;
        }

        public enum Button
        {
            Left,
            Right,
            Middle
        }
        public enum Action : int
        {
            Click,
            Down,
            Up,
            Move
        }

        //鼠标移动
        private static void Move(Point point)
        {
            HandleCoordinates(ref point);
            API.INPUT input = new API.INPUT();
            input.type = API.INPUTTYPE.MOUSE;
            input.mi.time = 0;
            input.mi.dx = point.X;
            input.mi.dy = point.Y;
            //移动的坐标包括所有显示器而非只有主显示器
            input.mi.dwFlags = API.MOUSEEVENTF.ABSOLUTE | API.MOUSEEVENTF.VIRTUALDESK | API.MOUSEEVENTF.MOVE;  
            input.mi.mouseData = 0;
            input.mi.dwExtraInfo = (UIntPtr)0x7FuL;
            API.SendInput(1u, ref input, Marshal.SizeOf(typeof(API.INPUT)));
        }

        //鼠标事件
        public static void Event(Point? point = null, Button button = Button.Left, Action action = Action.Click)
        {
            //错误时退出
            if ((action == Action.Move && !point.HasValue)//移动时必须有坐标
                || (point.HasValue&&(point.Value.X < 0 || point.Value.Y < 0))) //坐标非负
            {
                return;
            }
            //处理移动
            if (point.HasValue)
            {
                Move(point.Value);
            }
            if (action == Action.Move)
            {
                return;
            }
            //处理鼠标按键事件
            API.INPUT input = new API.INPUT();
            input.type = API.INPUTTYPE.MOUSE;
            input.mi.time = 0;
            input.mi.mouseData = 0;
            input.mi.dwExtraInfo = (UIntPtr)0x7FuL;   //Stroke规定的消息，附加以后不会被主程序hook回调函数处理
            //处理按键类别
            API.MOUSEEVENTF MOUSEDOWN, MOUSEUP;
            MOUSEDOWN = API.MOUSEEVENTF.LEFTDOWN; //默认左键
            MOUSEUP = API.MOUSEEVENTF.LEFTUP;
            switch (button)
            {
                case Button.Right:
                    MOUSEDOWN = API.MOUSEEVENTF.RIGHTDOWN;
                    MOUSEUP = API.MOUSEEVENTF.RIGHTUP;
                    break;
                case Button.Middle:
                    MOUSEDOWN = API.MOUSEEVENTF.MIDDLEDOWN;
                    MOUSEUP = API.MOUSEEVENTF.MIDDLEUP;
                    break;
                default:
                    break;
            }
            //处理按键动作
            switch (action)
            {
                case Action.Click:
                    input.mi.dwFlags = MOUSEDOWN | MOUSEUP;
                    break;
                case Action.Down:
                    input.mi.dwFlags = MOUSEDOWN;
                    break;
                case Action.Up:
                    input.mi.dwFlags = MOUSEUP;
                    break;
                default:
                    break;
            }
            //发送按键
            API.SendInput(1u, ref input, Marshal.SizeOf(typeof(API.INPUT)));
            return;
        }

        //返回一个point对象
        public static Point Point(int x, int y)
        {
            return new Point(x, y);
        }

        //返回当前鼠标位置
        public static Point Location()
        {
            return System.Windows.Forms.Cursor.Position;
        }

        //保存当前鼠标位置
        public static void SaveLocation()
        {
            Point location = Location();
            savedPoint.X = location.X;
            savedPoint.Y = location.Y;
            return;
        }

        //恢复之前保存的鼠标的位置
        public static void RestoreLocation()
        {
            if (savedPoint.X != -1)
            {
                Move(savedPoint);
            }
            return;
        }
    }
}
