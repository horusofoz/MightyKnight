using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace MightyKnight
{
    class Player
    {
        Sprite sprite = new Sprite();

        // need to keep a reference to the Game object so we can check for collisions on the map
        Game1 game = null;
        bool isFalling = true;
        bool isJumping = false;

        Vector2 velocity = Vector2.Zero;
        Vector2 position = Vector2.Zero;

        public Vector2 Position
        {
            get { return sprite.position; }
        }

        public Player(Game1 game)
        {
            this.game = game;
            isFalling = true;
            isJumping = false;
            velocity = Vector2.Zero;
            position = Vector2.Zero;
        }

        public void Load(ContentManager content)
        {
            AnimatedTexture animation = new AnimatedTexture(Vector2.Zero, 0, 1, 1);
            animation.Load(content, "sprites/walk", 12, 20);

            sprite.Add(animation, 0, -5);
            sprite.Pause();
        }

        public void Update(float deltaTime)
        {
            UpdateInput(deltaTime);
            sprite.Update(deltaTime);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            sprite.Draw(spriteBatch);
        }

        private void UpdateInput(float deltaTime)
        {
            bool wasMovingLeft = velocity.X < 0;
            bool wasMovingRight = velocity.X > 0;
            bool falling = isFalling;

            Vector2 acceleration = new Vector2(0, Game1.gravity);

            if (Keyboard.GetState().IsKeyDown(Keys.Left) == true)
            {
                acceleration.X -= Game1.acceleration;
                sprite.SetFlipped(true);
                sprite.Play();
            }
            else if(wasMovingLeft == true)
            {
                acceleration.X += Game1.friction;
                sprite.SetFlipped(false);
                sprite.Play();
            }
            if(Keyboard.GetState().IsKeyDown(Keys.Right) == true)
            {
                acceleration.X += Game1.acceleration;
            }
            else if(wasMovingRight == true)
            {
                acceleration.X -= Game1.friction;
            }

            // Jump
            if(Keyboard.GetState().IsKeyDown(Keys.Up) == true && this.isJumping == false && falling == false)
            {
                acceleration.Y -= Game1.jumpImpulse;
                this.isJumping = true;
            }

            // integrate the forces to calculate the new position and velocity
            velocity += acceleration * deltaTime;

            // Clamp the velocity so the player doesn't go too fast
            velocity.X = MathHelper.Clamp(velocity.X, -Game1.maxVelocity.X, Game1.maxVelocity.X);
            velocity.Y = MathHelper.Clamp(velocity.Y, -Game1.maxVelocity.Y, Game1.maxVelocity.Y);

            sprite.position += velocity * deltaTime;

            // Quuick fix to ensure player stops when not pressing a direction rather than friction pushing minutely left and right
            if((wasMovingLeft && (velocity.X > 0)) || (wasMovingRight && (velocity.X < 0)))
            {
                // clamp at zero to prevent frivtion from making us jiggle side to side
                velocity.X = 0;
                sprite.Pause();
            }

            // collision detection
            // Our collision detection logic is greatly simplified by the fact that
            // the player is a rectangle and is exactly the same size as a single tile.
            // So we know that the player can only ever occupy 1, 2 or 4 cells.
            // This means we can short-circuit and avoid building a general purpose
            // collision detection engine by simply looking at the 1 to 4 cells that
            // the player occupies:
            int tx = game.PixelToTile(sprite.position.X);
            int ty = game.PixelToTile(sprite.position.Y);
            bool nx = (sprite.position.X) % Game1.tile != 0; // nx = true if player overlaps to the right;
            bool ny = (sprite.position.Y) % Game1.tile != 0; // ny = true if player overlaps below;
            // cell, cellright, celldown and celldiag tell us which cell(s) around the player have collisions(platforms)
            bool cell = game.CellAtTileCoord(tx, ty) != 0;
            bool cellright = game.CellAtTileCoord(tx + 1, ty) != 0;
            bool celldown = game.CellAtTileCoord(tx, ty + 1) != 0;
            bool celldiag = game.CellAtTileCoord(tx + 1, ty + 1) != 0;

            // If the player has vertical veloicty, then check to see if they have hit a platform below or above, in which case, stop their vertical velocity and clamp their y position
            if(this.velocity.Y > 0)
            {
                if((celldown && !cell) || (celldiag && !cellright && nx))
                {
                    // clamp the y position to avoid falling into platform below
                    sprite.position.Y = game.TileToPixel(ty);
                    this.velocity.Y = 0;        // Stop downward velocity
                    this.isFalling = false;     // No longer falling
                    this.isJumping = false;     // No longer humping
                    ny = false;                 // No longer overlaps the cells below;

                }
            }
            else if(this.velocity.Y < 0)
            {
                if((cell && !celldown) || (cellright && !celldiag && nx))
                {
                    //clamp the Y position to avoid jumping in the platform above
                    sprite.position.Y = game.TileToPixel(ty + 1);
                    // Player is no longer really in that cell, we clamped them to the cell below
                    cell = celldown;
                    cellright = celldiag;
                    ny = false;     // player no longer overlaps the cells below
                }

                //Once the vertical velocity id taken care of, we can apply similar logic to the horizantal velocity:
                if(this.velocity.X > 0)
                {
                    if((cellright && !cell) || (celldiag && !celldown && ny))
                    {
                        // clamp the x position to avoid moving to the platform we just hit
                        sprite.position.X = game.TileToPixel(tx);
                        this.velocity.X = 0;    // Stop horizantal velocity
                        sprite.Pause();
                    }
                }
                else if(this.velocity.X < 0)
                {
                    if((cell && !cellright) || (celldown && !celldiag && ny))
                    {
                        // clamp the x position to avoid moving into the platform we just hit
                        sprite.position.X = game.TileToPixel(tx + 1);
                        this.velocity.X = 0;    // Stop horizantal velocity
                        sprite.Pause();
                    }
                }

                // The last calculation for our update() method is to detect if the player is now falling or not.
                // We can do that by looking to see if there is a platform below them;
                this.isFalling = !(celldown || (nx && celldiag));
            }
        }
    }
}
