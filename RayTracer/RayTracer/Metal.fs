﻿namespace Material
open Globals
open Hitable
open Ray
open Material.MaterialFuncs

type Metal(albedo:Vec3) = 
    let mutable emitted = Vec3(0.0,0.0,0.0)
    member this.SetEmitted (col:Vec3) = emitted <- col
    interface IMaterial with
        member this.Scatter(ray:Ray,record:HitRecord) rand : (Ray option*Vec3) =
            let reflected = Reflect ray.DirectionNorm record.Normal
            let scattered = Ray(record.Position,reflected)

            if Vector.Dot(scattered.Direction,record.Normal) > 0.0 then
                (Some(scattered),albedo)
            else
                (None,albedo)

        member this.Emitted with get() = emitted