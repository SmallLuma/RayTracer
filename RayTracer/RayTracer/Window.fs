﻿module Window

open System.Windows.Forms
open System
open Globals
open Hitable
open Ray
open Material
open Tracer
open Camera
open System.Drawing

[<Struct>]
type Image = {
    Width : int
    Height : int
    Pixels : Vec3[] }

let Gamma (col:Vec3) = 
    Vec3(Math.Sqrt(col.X),Math.Sqrt(col.Y),Math.Sqrt(col.Z))

let ToBitmap i =
    let bmp = new Bitmap(i.Width,i.Height)
    i.Pixels
    |> Array.iteri (fun pxid px ->
        let y = pxid / i.Width
        let x = pxid % i.Width
        bmp.SetPixel(x,y,Vec3ToDrawingColor (px |> Gamma)))
    bmp

let ReduceImage compressRate (imgs : Image[]) =
    let w,h =
        (Array.head imgs).Width,
        (Array.head imgs).Height
    {
        Width = w
        Height = h
        Pixels = Array.init (w*h) (fun pxid ->
            Seq.init (imgs.Length) (fun i ->
                imgs.[i].Pixels.[pxid])
            |> Seq.reduce (+)
            |> (*) (1.0/compressRate)) }

let DisplayImage (image:Drawing.Image) = 
    use window = new Form()
    window.Size <- image.Size
    window.Text <- "RayTracer"
    window.BackgroundImage <- image
    window.DoubleClick.Add(fun _ ->
        image.Save("Result.bmp")
        System.Diagnostics.Process.Start("Result.bmp") |> ignore)
    window.ShowDialog() |> ignore

let Render (size : Drawing.Size) (camera:Camera) (random:System.Random) colWeight target (objs : (IHitable*IMaterial) list)  = 
    let (xRecip,yRecip) = (1.0 / float size.Width,1.0 / float size.Height)
    for pxid in 0..size.Width*size.Height-1 do
        let y = pxid / size.Width
        let x = pxid % size.Width
        let xNorm = float x / float size.Width
        let yNorm = 1.0 - float y / float size.Height

        let xTrace = xNorm + random.NextDouble() * xRecip
        let yTrace = yNorm + random.NextDouble() * yRecip
        let ray =
            camera.CreateRay (xTrace,yTrace) random
        target.Pixels.[pxid] <-
            target.Pixels.[pxid] + (
                GetRayColor ray objs 0 5 random
                |> (*) colWeight)

