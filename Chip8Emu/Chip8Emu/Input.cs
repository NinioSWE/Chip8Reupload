using Microsoft.Xna.Framework.Input;
using System;


enum InputConversions
{
    X,
    _1,
    _2,
    _3,
    Q,
    W,
    E,
    A,
    S,
    D,
    Z,
    C,
    _4,
    R,
    F,
    V
}

public static class Input
{
    static KeyboardState state;

    public static bool[] InputsDown = new bool[16];

    public static void Update()
    {
        state = Keyboard.GetState();

        for (byte i = 0; i < 16; i++)
            InputsDown[i] = IsKeyDown(i);
    }

    public static bool IsKeyDown(byte key)
    {
        InputConversions keys = (InputConversions)key;
        switch (keys)
        {
            case InputConversions.X:
                return state.IsKeyDown(Keys.X);

            case InputConversions._1:
                return state.IsKeyDown(Keys.D1);

            case InputConversions._2:
                return state.IsKeyDown(Keys.D2);

            case InputConversions._3:
                return state.IsKeyDown(Keys.D3);

            case InputConversions.Q:
                return state.IsKeyDown(Keys.Q);

            case InputConversions.W:
                return state.IsKeyDown(Keys.W);

            case InputConversions.E:
                return state.IsKeyDown(Keys.E);

            case InputConversions.A:
                return state.IsKeyDown(Keys.A);

            case InputConversions.S:
                return state.IsKeyDown(Keys.S);

            case InputConversions.D:
                return state.IsKeyDown(Keys.D);

            case InputConversions.Z:
                return state.IsKeyDown(Keys.Z);

            case InputConversions.C:
                return state.IsKeyDown(Keys.C);

            case InputConversions._4:
                return state.IsKeyDown(Keys.D4);

            case InputConversions.R:
                return state.IsKeyDown(Keys.R);

            case InputConversions.F:
                return state.IsKeyDown(Keys.F);

            case InputConversions.V:
                return state.IsKeyDown(Keys.V);
        }

        return false;

    }
}

/*
public class C{
    void M(){
        bool b = (((ushort)Input.btn_0) << 15) == (ushort)Input.btn_15;
        //b är sann
        Input i = Input.btn_0 | Input.btn_1;
        //i är både btn 0 och btn 1
        

    }
}
*/
