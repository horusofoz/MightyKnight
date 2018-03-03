using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace MightyKnight
{
    class Player
    {
        Sprite sprite = new Sprite();

        public Player()
        {
        }

        public void Load(ContentManager content)
        {
            sprite.Load(content, "hero");
        }

        public void Update(float deltaTime)
        {
            sprite.Update(deltaTime);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            sprite.Draw(spriteBatch);
        }

    }
}
