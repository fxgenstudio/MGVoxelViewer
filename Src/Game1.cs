// MIT License - Copyright (C) FxGen 
// This file is subject to the terms and conditions defined in 
// file 'LICENSE.txt', which is part of this source code package. 

using FxVoxelUtils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.IO;

namespace VoxelViewer
{
  /// <summary>
  /// This is the main type for your game.
  /// </summary>
  public class Game1 : Game
  {
    GraphicsDeviceManager graphics;
    SpriteBatch spriteBatch;

    CubeBrush m_voxelBrush;
    OrbitCamera m_cam;
    float m_fAngle;

    public Game1()
    {
      graphics = new GraphicsDeviceManager(this);
      m_cam = new OrbitCamera(this);

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
      base.Initialize();

      m_cam.TargetDistance = 300;
    }

    /// <summary>
    /// LoadContent will be called once per game and is the place to load
    /// all of your content.
    /// </summary>
    protected override void LoadContent()
    {
      // Create a new SpriteBatch, which can be used to draw textures.
      spriteBatch = new SpriteBatch(GraphicsDevice);

      // Load Voxel Model from Content Application
      m_voxelBrush = ImportContentVoxelBrush("monu1.vox");
      
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

      //Camera 
      m_cam.Update(0);

      //Rotation
      m_fAngle += (float)(gameTime.ElapsedGameTime.Milliseconds / 1000.0);

      //Update VoxelBrush world Matrix
      if (m_voxelBrush != null)
      {
        var mtx = Matrix.CreateTranslation(m_voxelBrush.m_vec3Origin);
        mtx *= Matrix.CreateRotationY(m_fAngle);

        m_voxelBrush.m_worldMtx = mtx;
      }

      base.Update(gameTime);
    }

    /// <summary>
    /// This is called when the game should draw itself.
    /// </summary>
    /// <param name="gameTime">Provides a snapshot of timing values.</param>
    protected override void Draw(GameTime gameTime)
    {
      GraphicsDevice.Clear(Color.CornflowerBlue);

      // TODO: Add your drawing code here

      //Display Voxel Model
      if (m_voxelBrush != null)
      {
        m_voxelBrush.Draw(m_cam.View, m_cam.Projection);
      }

      base.Draw(gameTime);
    }


    /// <summary>
    /// Import Brush from Content Application
    /// </summary>
    /// <param name="_strName"></param>
    private CubeBrush ImportContentVoxelBrush(string _strName)
    {
      var stream = System.IO.File.OpenRead(Path.Combine(Content.RootDirectory, _strName));

      //Load From Content Application
      MagicaVoxelLoader loader = new MagicaVoxelLoader();
      BinaryReader br = new BinaryReader(stream);

      CubeBrush brush = null;

      if (loader.ReadFile(br) == true)
      {
        brush = new CubeBrush(this, loader.Array, loader.Palette);
        brush.GenerateMesh();
        brush.ApplyEditorEffect();
      }

      br.Dispose();

      return brush;
    }


  }
}
