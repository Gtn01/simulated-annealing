using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Force.DeepCloner;
using UnityEngine;

namespace packing3D{

    public class Annealable 
    {


        public static Model Anneal(Model in_state, float maxTemp, float minTemp, int steps, Action<Model, int , float, float> callback_progress){

            System.Diagnostics.Stopwatch debug_timer = new System.Diagnostics.Stopwatch();
            System.Diagnostics.Stopwatch debug_timer_doMove = new System.Diagnostics.Stopwatch();
            System.Diagnostics.Stopwatch debug_timer_energy = new System.Diagnostics.Stopwatch();
            System.Diagnostics.Stopwatch debug_timer_prepareNext = new System.Diagnostics.Stopwatch();
            debug_timer.Start();
            debug_timer_doMove.Start();
            debug_timer_energy.Start();
            debug_timer_prepareNext.Start();

            System.Random rand = new System.Random();

            bool isHeightAnnealing = !(in_state.printerWidth == -1 && in_state.printerHeight == -1 && in_state.printerDepth == -1);


            System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();
            timer.Start();
            float start = 0.0f;

            float factor = (float)(-Math.Log( maxTemp/minTemp ));

            Model state     = in_state.ShallowCopy();
            Model bestState = in_state.ShallowCopy();
          

            float bestEnergy = state.Energy(isHeightAnnealing);
            float previousEnergy = bestEnergy;
            float rate = steps / 200.0f;

            Model.Undo undo = new Model.Undo();

            callback_progress?.Invoke(bestState,0, bestEnergy, bestEnergy);

    
           debug_timer_doMove.Restart();
           debug_timer_energy.Restart();
           debug_timer_prepareNext.Restart();

            for(int step = 0;step < steps;step++){


                float pct  = (float)(step) / (float)(steps-1);
                float temp = (float)(maxTemp * Math.Pow( Math.Exp(factor*pct) , 3) );

                debug_timer_doMove.Start();
                state.DoMove(ref undo, isHeightAnnealing);
                debug_timer_doMove.Stop();


                debug_timer_energy.Start();
                float energy = state.Energy(isHeightAnnealing);
                debug_timer_energy.Stop();

                float change    = energy - previousEnergy;

                double mutation = Math.Exp(-change/temp);

                if(step%rate == 0) {

                    showProgress(step, steps, bestEnergy,  energy, timer.Elapsed.Seconds,  "["+debug_timer.ElapsedMilliseconds+"ms ] "+" [ doMove : "+debug_timer_doMove.ElapsedMilliseconds+"ms ] "+" [ energy : "+debug_timer_energy.ElapsedMilliseconds+"ms ] [ end "+debug_timer_prepareNext.ElapsedMilliseconds+"ms ]"/*+" min : "+state.BoundingBox().Min.x+","+state.BoundingBox().Min.y+","+state.BoundingBox().Min.z+" max : "+state.BoundingBox().Max.x+","+state.BoundingBox().Max.y+","+state.BoundingBox().Max.z*/  );
                    callback_progress?.Invoke(null, (int)(100 * (float)(step) / (float)(steps)), bestEnergy, energy);
                  
                   debug_timer.Restart();
                   debug_timer_doMove.Reset();
                   debug_timer_energy.Reset();
                   debug_timer_prepareNext.Reset();
                   debug_timer_doMove.Stop();
                   debug_timer_energy.Stop();
                   debug_timer_prepareNext.Stop();
                    
                }

                debug_timer_prepareNext.Start();


                if ( change > 0 &&  mutation < Util.RandTinyFloat(rand) ){
                    
                    state.UndoMove(undo);

                } else {

                    previousEnergy = energy;

                    if ( energy < bestEnergy) {

                        bestEnergy = energy;
                        bestState = state.ShallowCopy();

                        callback_progress?.Invoke(bestState,  (int)(100 * (float)(step) / (float)(steps)), bestEnergy, energy);
                    }
                }

                if( Main._abort3DPacking ){//termine ici pour finir la fonction et retourner le meilleur état
                    break;
                }

                debug_timer_prepareNext.Stop();

                
            }

            return bestState;
        }

        public static void showProgress(int i , int n , float e, float ce, float d, string others ) {

            float pct = (int)(100 * (float)(i) / (float)(n));

            StringBuilder fmt = new StringBuilder();
            fmt.Append(pct+"% [");

            for(int p = 0;p<100;p+=3){
                if(pct > p){
                    fmt.Append("=");
                }else{
                    fmt.Append(" ");
                }
            }

            fmt.Append(" ] best energy : "+e+"  currentenergy : "+ce);
            Debug.Log(fmt.ToString());

        }

    }
    
}
