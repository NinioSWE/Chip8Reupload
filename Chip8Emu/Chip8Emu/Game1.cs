using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Linq;

namespace Chip8Emu
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        CPU cpu = new CPU();

        Texture2D texture;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            base.Initialize();

            texture = new Texture2D(GraphicsDevice, 64 , 32);
            graphics.PreferredBackBufferWidth = 32 * 32 + 40;  // set this value to the desired width of your window
            graphics.PreferredBackBufferHeight = 64 * 32;   // set this value to the desired height of your window
            graphics.ApplyChanges();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            cpu.LoadGame("Soccer.ch8");

            // TODO: use this.Content to load your game content here
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here

            base.Update(gameTime);
        }



        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Blue);
            Input.Update();
            cpu.EmulateCycle();

            Color[] colors = new Color[64 * 32];
            for (int y = 0; y < 32; y++)
            {
                for (int x = 0; x < 64; x++)
                {
                    //var c = GetColors(cpu.pixels[x + y * 64]);
                    var c = cpu.pixels[x + y * 64] == 1 ? Color.White: Color.Black;
                    colors[ x + y * 64] = c;
                }
            }

            //Console.WriteLine(string.Join(", ", colors.Select((c, i) => new C { c=c, i=i}).Where(c => c.c != Color.White).Select(c => c.i).ToArray()));


            texture.SetData<Color>(colors);

            //Draw texture to screen
            spriteBatch.Begin(samplerState:SamplerState.PointClamp);
            spriteBatch.Draw(texture,new Rectangle(0,0,graphics.PreferredBackBufferWidth,graphics.PreferredBackBufferHeight),new Rectangle(0,0,64,32), Color.White);
            spriteBatch.End();


            base.Draw(gameTime);
        }

        /*Color[] GetColors(byte b)
        {
            Color[] cs = new Color[8];

            byte bitMask = 1;
            for (int i = 0; i < 8; i++)
            {
                // 1101 1011 & 0000 0001 == 1
                // 1101 1011 & 0000 0010 == 10
                if ((b & bitMask) == bitMask)
                    cs[i] = Color.Yellow;

                bitMask <<= 1;
            }
            return cs;
        }*/
    }


}

struct C{
    public Color c;
    public int i;

}
