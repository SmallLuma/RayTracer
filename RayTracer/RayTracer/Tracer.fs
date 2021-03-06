﻿module Tracer

open Globals
open Ray
open Hitable
open Material
open System

type TraceRecord = { 
    HitRecord : HitRecord
    Material : IMaterial }

let Background (ray:Ray) =
    let t = 0.5 * ray.DirectionNorm.Y + 1.0
    (1.0 - t) * Vec3(1.0,1.0,1.0) + t * Vec3(0.5,0.7,1.0)


let Trace (ray : Ray) (tmin:float) (tmax:float) (objs : (IHitable*IMaterial) list) : TraceRecord option =
    let mutable closest = tmax
    let mutable record : TraceRecord option = None
    for (hitable,mat) in objs do
        match hitable.Hit(ray,tmin,closest) with
        | Some(hitRecord) -> 
            closest <- hitRecord.RayT
            record <- Some({HitRecord=hitRecord;Material=mat})
        | None -> ()
    record

let rec GetRayColor (ray:Ray) (objs:(IHitable*IMaterial) list) depth maxDepth rand : Vec3 =
    match Trace ray 0.0000001 Double.MaxValue objs with
    | Some(record) -> 
        let (ray,atten) = record.Material.Scatter (ray,record.HitRecord) rand
        if ray.IsSome && depth < maxDepth then
            let col = GetRayColor ray.Value objs (depth+1) maxDepth rand
            Vec3(col.X * atten.X,col.Y * atten.Y,col.Z * atten.Z) + record.Material.Emitted
        else
            record.Material.Emitted
    | None -> Background ray
