using System;
using System.Collections.Generic;
using System.Text;

namespace TankWars
{
    class CollisionDetection
    {
        /// <summary>
        /// Calculates "space" which determines how much padding you want to include. For tanks, you want 
        /// the tank to stop outside the wall, not under or on it.
        /// Returns true if you collide with wall, false if you didn't.
        /// </summary>
        /// <param name="collisionItem"></param>
        /// <param name="loc"></param>
        /// <param name="world"></param>
        /// <returns></returns>
        public static bool ObjectToWallCollision(Object collisionItem, Vector2D loc, World world)
        {
            double space;
            if (collisionItem is Projectile || collisionItem is Powerup)
            {
                space = world.wallsize / 2;
            }
            else
            {
                space = world.wallsize / 2 + world.tanksize / 2;
            }

            foreach (Wall wall in world.Walls.Values)
            {
                Vector2D wallDirection = wall.EP1 - wall.EP2;
                bool collide;
                if (wallDirection.GetX() == 0)
                {
                    double x1 = wall.EP1.GetX() - space;
                    double x2 = wall.EP1.GetX() + space;
                    double y1 = Math.Min(wall.EP1.GetY(), wall.EP2.GetY()) - space;
                    double y2 = Math.Max(wall.EP1.GetY(), wall.EP2.GetY()) + space;

                    collide = DidCollide(x1, x2, y1, y2, loc);

                }
                else
                {
                    double y1 = wall.EP1.GetY() - space;
                    double y2 = wall.EP1.GetY() + space;
                    double x1 = Math.Min(wall.EP1.GetX(), wall.EP2.GetX()) - space;
                    double x2 = Math.Max(wall.EP1.GetX(), wall.EP2.GetX()) + space;
                    collide = DidCollide(x1, x2, y1, y2, loc);
                }

                if (collide)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Updates model data when tank is hit by projectile.
        /// Updates model data when tank hits a powerup.
        /// Returns boolean on whether collisionItem(powerup/projectile) collides with tank.
        /// </summary>
        /// <param name="collisionItem"></param>
        /// <param name="world"></param>
        /// <returns></returns>
        public static bool ObjectToTankCollision(Object collisionItem, World world, List<int> removeList)
        {
            lock (world.Tanks)
            {
                foreach (Tank tank in world.Tanks.Values)
                {
                    bool collide = false;
                    double x1 = tank.loc.GetX() - world.tanksize / 2;
                    double x2 = tank.loc.GetX() + world.tanksize / 2;
                    double y1 = tank.loc.GetY() - world.tanksize / 2;
                    double y2 = tank.loc.GetY() + world.tanksize / 2;

                    if (collisionItem is Projectile)
                    {
                        Projectile proj = (Projectile)collisionItem;
                        collide = DidCollide(x1, x2, y1, y2, proj.loc);
                        if (collide && (tank.tank != proj.Owner))
                        {
                            if (tank.hp > 0)
                            {
                                tank.hp--;
                                if (tank.hp == 0)
                                {
                                    tank.Died = true;
                                    tank.waitToSpawn = 0;
                                    world.Tanks[proj.Owner].Score++;
                                }
                            }

                            proj.died = true;
                            return true;
                        }
                    }

                    if (collisionItem is Powerup)
                    {
                        Powerup pwrUp = (Powerup)collisionItem;
                        collide = DidCollide(x1, x2, y1, y2, pwrUp.loc);
                        if (collide)
                        {
                            tank.beamFire = tank.beamFire + 1;
                            pwrUp.died = true;
                            removeList.Add(pwrUp.ID);
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Updates owner tank and hit tank when beam collides with a tank.
        /// Adds score to owner tank.
        /// Kills hit tank and reduces hit points to 0.
        /// </summary>
        /// <param name="beam"></param>
        /// <param name="world"></param>
        /// <returns></returns>
        public static bool BeamToTankCollision(Beam beam, World world)
        {
            Tank owner = world.Tanks[beam.owner];
            bool isHit = false;
            foreach (Tank t in world.Tanks.Values)
            {
                if (t.tank != beam.owner)
                {
                    bool hit = Intersects(owner.loc, beam.direction, t.loc, world.tanksize / 2);
                    if (hit)
                    {
                        t.hp = 0;
                        t.Died = true;
                        t.waitToSpawn = 0;
                        owner.Score++;
                        isHit = true;
                    }
                }
            }
            return isHit;
        }

        /// <summary>
        /// Returns true if a beam goes through center of object.
        /// </summary>
        /// <param name="beamOrig"></param>
        /// <param name="beamDir"></param>
        /// <param name="center"></param>
        /// <param name="r"></param>
        /// <returns></returns>
        public static bool Intersects(Vector2D beamOrig, Vector2D beamDir, Vector2D center, double areaSize)
        {
            double a = beamDir.Dot(beamDir);
            double b = ((beamOrig - center) * 2.0).Dot(beamDir);
            double c = (beamOrig - center).Dot(beamOrig - center) - areaSize * areaSize;

            double disc = b * b - 4.0 * a * c;

            if (disc < 0.0)
                return false;

            double root1 = -b + Math.Sqrt(disc);
            double root2 = -b - Math.Sqrt(disc);

            return (root1 > 0.0 && root2 > 0.0);
        }

        /// <summary>
        /// Returns true if two items collide.
        /// Does a series of comparisons to determine a collision.
        /// </summary>
        /// <param name="x1"></param>
        /// <param name="x2"></param>
        /// <param name="y1"></param>
        /// <param name="y2"></param>
        /// <param name="input"></param>
        /// <returns></returns>
        private static bool DidCollide(double x1, double x2, double y1, double y2, Vector2D input) => x1 <= input.GetX() && input.GetX() <= x2 && y1 <= input.GetY() && input.GetY() <= y2;

    }
}
