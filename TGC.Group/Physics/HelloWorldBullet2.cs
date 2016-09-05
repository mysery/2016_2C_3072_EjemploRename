using System.Drawing;
using TGC.Core.Camara;
using TGC.Core.Geometry;
using TGC.Core.UserControls;
using TGC.Core.UserControls.Modifier;
using TGC.Core.Textures;
using TGC.Core.Utils;
using Microsoft.DirectX.DirectInput;
using BulletSharp;
using TGC.Core.Direct3D;
using TGC.Group.Model;
using TGC.Group.Wrappers;
using System.Collections.Generic;
using System;

namespace TGC.Group.Physics
{
    public class HelloWorldBullet2 : PhysicsGame
    {
        private TgcPlane floorMesh;
        private TgcBox boxMesh;
        private TgcSphere sphereMesh;
        

        //Rigid Bodies:
        RigidBody floorBody;        
        List<RigidBody> ballBodys = new List<RigidBody>();
        List<RigidBody> boxBodys = new List<RigidBody>();


        public override void Init(GameModel ctx)
        {
            base.Init(ctx);
            
            //Creamos shapes y bodies.

            //El piso es un plano estatico se dice que si tiene masa 0 es estatico.
            var floorShape = new StaticPlaneShape(new Vector(0, 1, 0).ToBsVector, 0);
            var floorMotionState = new DefaultMotionState();
            var floorInfo = new RigidBodyConstructionInfo(0, floorMotionState, floorShape);
            floorBody = new RigidBody(floorInfo);
            dynamicsWorld.AddRigidBody(floorBody);

            var boxBody = this.CreateBox(10f, 1f, 10f, 100f, 10f, MathUtil.SIMD_HALF_PI, MathUtil.SIMD_QUARTER_PI, MathUtil.SIMD_2_PI);
            boxBodys.Add(boxBody);
            dynamicsWorld.AddRigidBody(boxBody);

            var ballBody = this.CreateBall(10f, 1f, 0f, 50f, 0f);
            ballBodys.Add(ballBody);
            dynamicsWorld.AddRigidBody(ballBody);


            //Cargamos objetos de render del framework.
            var floorTexture = TgcTexture.createTexture(D3DDevice.Instance.Device, Ctx.MediaDir + "Texturas\\granito.jpg");
            floorMesh = new TgcPlane(new Vector(-2000, 0, -2000).ToDxVector, new Vector(4000, 0f, 4000).ToDxVector, TgcPlane.Orientations.XZplane, floorTexture);

            var texture = TgcTexture.createTexture(D3DDevice.Instance.Device, Ctx.MediaDir + "\\Texturas\\boxMetal.jpg");
            //Es importante crear todos los mesh con centro en el 0,0,0 y que este coincida con el centro de masa definido caso contrario rotaria de otra forma diferente a la dada por el motor de fisica.
            boxMesh = TgcBox.fromSize(new Vector(20, 20, 20).ToDxVector, texture);

            texture = TgcTexture.createTexture(D3DDevice.Instance.Device, Ctx.MediaDir + "\\Texturas\\pokeball.jpg");
            //Se crea una esfera de tamaño 1 para escalarla luego (en render)
            sphereMesh = new TgcSphere(1, texture, new Vector(0, 0, 0).ToDxVector);
            //Tgc no crea el vertex buffer hasta invocar a update values.
            sphereMesh.updateValues();
        }

        public override void Update()
        {
            dynamicsWorld.StepSimulation(1 / 60f, 10);

            if (Ctx.Input.keyUp(Key.A))
            {
                var ballBody = this.CreateBall(10f, 1f, 0f, 100f, 0f);
                ballBodys.Add(ballBody);
                dynamicsWorld.AddRigidBody(ballBody);
            }

            if (Ctx.Input.keyUp(Key.S))
            {
                var boxBody = this.CreateBox(10f, 1f, 5f, 150f, 5f, MathUtil.SIMD_HALF_PI, MathUtil.SIMD_QUARTER_PI, MathUtil.SIMD_2_PI);
                boxBodys.Add(boxBody);
                dynamicsWorld.AddRigidBody(boxBody);
            }

            if (Ctx.Input.keyUp(Key.Space))
            {
                var ballBody = this.CreateBall(10f, 1f, Ctx.Camara.Position.X, Ctx.Camara.Position.Y, Ctx.Camara.Position.Z);
                ballBody.LinearVelocity = new Vector(-Ctx.Camara.Position.X, -Ctx.Camara.Position.Y, -Ctx.Camara.Position.Z).ToBsVector * 0.2f;
                ballBody.Restitution = 0.9f;
                ballBodys.Add(ballBody);
                dynamicsWorld.AddRigidBody(ballBody);
            }

        }

        /// <summary>
        ///     M�todo que se invoca todo el tiempo. Es el render-loop de una aplicaci�n gr�fica.
        ///     En este m�todo se deben dibujar todos los objetos que se desean mostrar.
        ///     Antes de llamar a este m�todo el framework limpia toda la pantalla.
        ///     Por lo tanto para que un objeto se vea hay volver a dibujarlo siempre.
        ///     La variable elapsedTime indica la cantidad de segundos que pasaron entre esta invocaci�n
        ///     y la anterior de render(). Es �til para animar e interpolar valores.
        /// </summary>
        public override void Render()
        {
            
            foreach (RigidBody boxBody in boxBodys)
            {
                //Obtenemos la matrix de directx con la transformacion que corresponde a la caja.
                boxMesh.Transform = new Matrix(boxBody.InterpolationWorldTransform).ToDxMatrix;
                //Dibujar las cajas en pantalla
                boxMesh.render();
            }

            foreach (RigidBody ballBody in ballBodys)
            {
                //Obtenemos la transformacion de la pelota, en este caso se ve como se puede escalar esa transformacion.            
                sphereMesh.Transform = Matrix.Scaling(10, 10, 10).ToDxMatrix * new Matrix(ballBody.InterpolationWorldTransform).ToDxMatrix;
                sphereMesh.render();
            }

            floorMesh.render();
        }

        /// <summary>
        ///     M�todo que se invoca una sola vez al finalizar el ejemplo.
        ///     Se debe liberar la memoria de todos los recursos utilizados.
        /// </summary>
        public override void Dispose()
        {
            //Liberar memoria de las cajas 3D.
            //Por mas que estamos en C# con Garbage Collector igual hay que liberar la memoria de los recursos gráficos.
            //Porque están utilizando memoria de la placa de video (y ahí no hay Garbage Collector).
            dynamicsWorld.Dispose();
            dispatcher.Dispose();
            collisionConfiguration.Dispose();
            constraintSolver.Dispose();
            overlappingPairCache.Dispose();
            foreach (RigidBody boxBody in boxBodys)
            {
                boxBody.Dispose();
            }
            foreach (RigidBody ballBody in ballBodys)
            {
                ballBody.Dispose();
            }            
            floorBody.Dispose();

            boxMesh.dispose();
            sphereMesh.dispose();
            floorMesh.dispose();
        }
    }
}
