﻿namespace Drone.Core.LowLevel.servo
{
    internal class servo
    {
        // Valeur en microsec à envoyer au servo pour reset le moteur. Defaut = 90° = 9000us = Vertical              => TEST VALUES   // TODO
        public static int ZeroValue = 9000;

        public static int angleToValue(int angle)
        {
            // TODO : Trouver la fonction qui transforme l'angle en valeur à rentrer dans le drone pour orienter les hélices
            return angle*100;
        }
    }
}