using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ODIF;
using ODIF.Extensions;
using SlimDX;
using SlimDX.XInput;
using System.Threading;
using System.Management;

namespace _360_Controller_Input
{
    [PluginInfo(
        PluginName = "360 Controller Input",
        PluginDescription = "",
        PluginID = 12,
        PluginAuthorName = "InputMapper",
        PluginAuthorEmail = "jhebbel@gmail.com",
        PluginAuthorURL = "http://inputmapper.com",
        PluginIconPath = @"pack://application:,,,/360 Controller Input;component/Resources/360_Guide.png"
    )]
    public class Xbox360ControllerPlugin : InputDevicePlugin
    {
        Controller controller1 = new Controller(UserIndex.One);
        Controller controller2 = new Controller(UserIndex.Two);
        Controller controller3 = new Controller(UserIndex.Three);
        Controller controller4 = new Controller(UserIndex.Four);

        public Xbox360ControllerPlugin()
        {
            Global.HardwareChangeDetected += CheckForControllersEvent;
            CheckForControllers();
        }
        private void CheckForControllersEvent(object sender, EventArrivedEventArgs e)
        {
            CheckForControllers();
        }
        private void CheckForControllers()
        {
                if (controller1.IsConnected && Devices.OfType<my360Device>().Where(d => d.ControllerID == UserIndex.One).Count() == 0)
                    Devices.Add(new my360Device(UserIndex.One));
                else if (!controller1.IsConnected && Devices.OfType<my360Device>().Where(d => d.ControllerID == UserIndex.One).Count() > 0)
                    Devices.Remove(Devices.OfType<my360Device>().Where(d => d.ControllerID == UserIndex.One).First());

                if (controller2.IsConnected && Devices.OfType<my360Device>().Where(d => d.ControllerID == UserIndex.Two).Count() == 0)
                    Devices.Add(new my360Device(UserIndex.Two));
                else if (!controller2.IsConnected && Devices.OfType<my360Device>().Where(d => d.ControllerID == UserIndex.Two).Count() > 0)
                    Devices.Remove(Devices.OfType<my360Device>().Where(d => d.ControllerID == UserIndex.Two).First());

                if (controller3.IsConnected && Devices.OfType<my360Device>().Where(d => d.ControllerID == UserIndex.Three).Count() == 0)
                    Devices.Add(new my360Device(UserIndex.Three));
                else if (!controller3.IsConnected && Devices.OfType<my360Device>().Where(d => d.ControllerID == UserIndex.Three).Count() > 0)
                    Devices.Remove(Devices.OfType<my360Device>().Where(d => d.ControllerID == UserIndex.Three).First());

                if (controller4.IsConnected && Devices.OfType<my360Device>().Where(d => d.ControllerID == UserIndex.Four).Count() == 0)
                    Devices.Add(new my360Device(UserIndex.Four));
                else if (!controller4.IsConnected && Devices.OfType<my360Device>().Where(d => d.ControllerID == UserIndex.Four).Count() > 0)
                    Devices.Remove(Devices.OfType<my360Device>().Where(d => d.ControllerID == UserIndex.Four).First());
        }
    }

    public class my360Device : InputDevice
    {
        Thread ListenerThread;
        internal UserIndex ControllerID;
        x360device device;
        Controller controller;

        public my360Device(UserIndex ControllerID)
        {
            this.ControllerID = ControllerID;
            this.DeviceName = "Controller " + ControllerID.ToString();

            device = new x360device();
            controller = new Controller(ControllerID);

            if (controller.GetCapabilities(DeviceQueryType.Any).Subtype.HasFlag(DeviceSubtype.Gamepad))
                this.StatusIcon = Properties.Resources.controller.ToImageSource();

            Channels.Add(device.LSx);
            Channels.Add(device.LSy);
            Channels.Add(device.RSx);
            Channels.Add(device.RSy);

            Channels.Add(device.LS);
            Channels.Add(device.RS);

            Channels.Add(device.LT);
            Channels.Add(device.RT);
            Channels.Add(device.LB);
            Channels.Add(device.RB);

            Channels.Add(device.DUp);
            Channels.Add(device.DDown);
            Channels.Add(device.DLeft);
            Channels.Add(device.DRight);

            Channels.Add(device.A);
            Channels.Add(device.B);
            Channels.Add(device.X);
            Channels.Add(device.Y);

            Channels.Add(device.Start);
            Channels.Add(device.Back);

            Channels.Add(device.SmallRumble);
            Channels.Add(device.BigRumble);

            ListenerThread = new Thread(inputListener);
            ListenerThread.Start();
        }
        private void inputListener()
        {
            while (controller.IsConnected)
            {
                lock (controller)
                {
                    try
                    {
                        System.Threading.Thread.Sleep(1);
                        State state = controller.GetState();
                        device.LSx.Value = state.Gamepad.LeftThumbX / 32767f;
                        device.LSy.Value = state.Gamepad.LeftThumbY / -32767f;
                        device.RSx.Value = state.Gamepad.RightThumbX / 32767f;
                        device.RSy.Value = state.Gamepad.RightThumbY / -32767f;

                        device.LS.Value = state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.LeftThumb);
                        device.RS.Value = state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.RightThumb);

                        device.LB.Value = state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.LeftShoulder);
                        device.RB.Value = state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.RightShoulder);

                        device.LT.Value = state.Gamepad.LeftTrigger / 255f;
                        device.RT.Value = state.Gamepad.RightTrigger / 255f;

                        device.DUp.Value = state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.DPadUp);
                        device.DDown.Value = state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.DPadDown);
                        device.DLeft.Value = state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.DPadLeft);
                        device.DRight.Value = state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.DPadRight);

                        device.A.Value = state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.A);
                        device.B.Value = state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.B);
                        device.X.Value = state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.X);
                        device.Y.Value = state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.Y);

                        device.Start.Value = state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.Start);
                        device.Back.Value = state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.Back);
                        Vibration vibe = new Vibration();
                        vibe.LeftMotorSpeed = (ushort)device.SmallRumble.Value;
                        vibe.RightMotorSpeed = (ushort)device.BigRumble.Value;

                        controller.SetVibration(vibe);
                    }
                    catch { }
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (ListenerThread != null && ListenerThread.IsAlive)
                ListenerThread.Abort();
            base.Dispose(disposing);
        }
    }

    internal class x360device
    {
        public JoyAxis LSx { get; set; }
        public JoyAxis LSy { get; set; }
        public JoyAxis RSx { get; set; }
        public JoyAxis RSy { get; set; }

        public Button LS { get; set; }
        public Button RS { get; set; }

        public JoyAxis LT { get; set; }
        public JoyAxis RT { get; set; }
        public Button LB { get; set; }
        public Button RB { get; set; }

        public Button DUp { get; set; }
        public Button DDown { get; set; }
        public Button DLeft { get; set; }
        public Button DRight { get; set; }

        public Button A { get; set; }
        public Button B { get; set; }
        public Button X { get; set; }
        public Button Y { get; set; }

        public Button Start { get; set; }
        public Button Back { get; set; }
        public Button Guide { get; set; }

        public RumbleMotor BigRumble { get; set; }
        public RumbleMotor SmallRumble { get; set; }

        public x360device()
        {
            LSx = new JoyAxis("Left Stick X", DataFlowDirection.Input, "", Properties.Resources._360_Left_Stick.ToImageSource());
            LSy = new JoyAxis("Left Stick Y", DataFlowDirection.Input, "", Properties.Resources._360_Left_Stick.ToImageSource());
            RSx = new JoyAxis("Right Stick X", DataFlowDirection.Input, "", Properties.Resources._360_Right_Stick.ToImageSource());
            RSy = new JoyAxis("Right Stick Y", DataFlowDirection.Input, "", Properties.Resources._360_Right_Stick.ToImageSource());

            LS = new Button("Left Stick", DataFlowDirection.Input, "", Properties.Resources._360_Left_Stick.ToImageSource());
            RS = new Button("Right Stick", DataFlowDirection.Input, "", Properties.Resources._360_Right_Stick.ToImageSource());

            LT = new JoyAxis("Left Trigger", DataFlowDirection.Input, "", Properties.Resources._360_LT.ToImageSource()) { min_Value = 0 };
            RT = new JoyAxis("Right Trigger", DataFlowDirection.Input, "", Properties.Resources._360_RT.ToImageSource()) { min_Value = 0 };
            LB = new Button("Left Bumper", DataFlowDirection.Input, "", Properties.Resources._360_LB.ToImageSource());
            RB = new Button("Right Bumper", DataFlowDirection.Input, "", Properties.Resources._360_RB.ToImageSource());

            DUp = new Button("DPad Up", DataFlowDirection.Input, "", Properties.Resources._360_Dpad_Up.ToImageSource());
            DDown = new Button("DPad Down", DataFlowDirection.Input, "", Properties.Resources._360_Dpad_Down.ToImageSource());
            DLeft = new Button("DPad Left", DataFlowDirection.Input, "", Properties.Resources._360_Dpad_Left.ToImageSource());
            DRight = new Button("DPad Right", DataFlowDirection.Input, "", Properties.Resources._360_Dpad_Right.ToImageSource());

            A = new Button("A", DataFlowDirection.Input, "", Properties.Resources._360_A.ToImageSource());
            B = new Button("B", DataFlowDirection.Input, "", Properties.Resources._360_B.ToImageSource());
            X = new Button("X", DataFlowDirection.Input, "", Properties.Resources._360_X.ToImageSource());
            Y = new Button("Y", DataFlowDirection.Input, "", Properties.Resources._360_Y.ToImageSource());

            Start = new Button("Start", DataFlowDirection.Input, "", Properties.Resources._360_Start.ToImageSource());
            Back = new Button("Back", DataFlowDirection.Input, "", Properties.Resources._360_Back.ToImageSource());
            Guide = new Button("Guide", DataFlowDirection.Input, "", Properties.Resources._360_Guide.ToImageSource());

            BigRumble = new RumbleMotor("Big Rumble", DataFlowDirection.Output, "");
            SmallRumble = new RumbleMotor("Small Rumble", DataFlowDirection.Output, "");
        }
    }
}
