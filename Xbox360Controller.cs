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

            InputChannels.Add(device.LSx);
            InputChannels.Add(device.LSy);
            InputChannels.Add(device.RSx);
            InputChannels.Add(device.RSy);

            InputChannels.Add(device.LS);
            InputChannels.Add(device.RS);

            InputChannels.Add(device.LT);
            InputChannels.Add(device.RT);
            InputChannels.Add(device.LB);
            InputChannels.Add(device.RB);

            InputChannels.Add(device.DUp);
            InputChannels.Add(device.DDown);
            InputChannels.Add(device.DLeft);
            InputChannels.Add(device.DRight);

            InputChannels.Add(device.A);
            InputChannels.Add(device.B);
            InputChannels.Add(device.X);
            InputChannels.Add(device.Y);

            InputChannels.Add(device.Start);
            InputChannels.Add(device.Back);

            OutputChannels.Add(device.SmallRumble);
            OutputChannels.Add(device.BigRumble);

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
        public InputChannelTypes.JoyAxis LSx { get; set; }
        public InputChannelTypes.JoyAxis LSy { get; set; }
        public InputChannelTypes.JoyAxis RSx { get; set; }
        public InputChannelTypes.JoyAxis RSy { get; set; }

        public InputChannelTypes.Button LS { get; set; }
        public InputChannelTypes.Button RS { get; set; }

        public InputChannelTypes.JoyAxis LT { get; set; }
        public InputChannelTypes.JoyAxis RT { get; set; }
        public InputChannelTypes.Button LB { get; set; }
        public InputChannelTypes.Button RB { get; set; }

        public InputChannelTypes.Button DUp { get; set; }
        public InputChannelTypes.Button DDown { get; set; }
        public InputChannelTypes.Button DLeft { get; set; }
        public InputChannelTypes.Button DRight { get; set; }

        public InputChannelTypes.Button A { get; set; }
        public InputChannelTypes.Button B { get; set; }
        public InputChannelTypes.Button X { get; set; }
        public InputChannelTypes.Button Y { get; set; }

        public InputChannelTypes.Button Start { get; set; }
        public InputChannelTypes.Button Back { get; set; }
        public InputChannelTypes.Button Guide { get; set; }

        public OutputChannelTypes.RumbleMotor BigRumble { get; set; }
        public OutputChannelTypes.RumbleMotor SmallRumble { get; set; }

        public x360device()
        {
            LSx = new InputChannelTypes.JoyAxis("Left Stick X", "", Properties.Resources._360_Left_Stick.ToImageSource());
            LSy = new InputChannelTypes.JoyAxis("Left Stick Y", "", Properties.Resources._360_Left_Stick.ToImageSource());
            RSx = new InputChannelTypes.JoyAxis("Right Stick X", "", Properties.Resources._360_Right_Stick.ToImageSource());
            RSy = new InputChannelTypes.JoyAxis("Right Stick Y", "", Properties.Resources._360_Right_Stick.ToImageSource());

            LS = new InputChannelTypes.Button("Left Stick", "", Properties.Resources._360_Left_Stick.ToImageSource());
            RS = new InputChannelTypes.Button("Right Stick", "", Properties.Resources._360_Right_Stick.ToImageSource());

            LT = new InputChannelTypes.JoyAxis("Left Trigger", "", Properties.Resources._360_LT.ToImageSource()) { min_Value = 0 };
            RT = new InputChannelTypes.JoyAxis("Right Trigger", "", Properties.Resources._360_RT.ToImageSource()) { min_Value = 0 };
            LB = new InputChannelTypes.Button("Left Bumper", "", Properties.Resources._360_LB.ToImageSource());
            RB = new InputChannelTypes.Button("Right Bumper", "", Properties.Resources._360_RB.ToImageSource());

            DUp = new InputChannelTypes.Button("DPad Up", "", Properties.Resources._360_Dpad_Up.ToImageSource());
            DDown = new InputChannelTypes.Button("DPad Down", "", Properties.Resources._360_Dpad_Down.ToImageSource());
            DLeft = new InputChannelTypes.Button("DPad Left", "", Properties.Resources._360_Dpad_Left.ToImageSource());
            DRight = new InputChannelTypes.Button("DPad Right", "", Properties.Resources._360_Dpad_Right.ToImageSource());

            A = new InputChannelTypes.Button("A", "", Properties.Resources._360_A.ToImageSource());
            B = new InputChannelTypes.Button("B", "", Properties.Resources._360_B.ToImageSource());
            X = new InputChannelTypes.Button("X", "", Properties.Resources._360_X.ToImageSource());
            Y = new InputChannelTypes.Button("Y", "", Properties.Resources._360_Y.ToImageSource());

            Start = new InputChannelTypes.Button("Start", "", Properties.Resources._360_Start.ToImageSource());
            Back = new InputChannelTypes.Button("Back", "", Properties.Resources._360_Back.ToImageSource());
            Guide = new InputChannelTypes.Button("Guide", "", Properties.Resources._360_Guide.ToImageSource());

            BigRumble = new OutputChannelTypes.RumbleMotor("Big Rumble", "");
            SmallRumble = new OutputChannelTypes.RumbleMotor("Small Rumble", "");
        }
    }
}
