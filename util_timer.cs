using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class util_timer
{

    #region PRIVATE
    public static Dictionary<string, util_timer> timers = new Dictionary<string, util_timer>();
    private static int ID = 0;
    private static float COOLDOWN_ID = -1;

    #region TIMER
    private string _id;
    private float _nextTick;
    private float _delay;
    private int _iterations;
    private Action<int> _func;
    private Action _onComplete;
    private float _pausedTime;
    private bool _paused;
    private bool _infinite;
    #endregion
    #endregion

    public static void fixedUpdate()
    {
        // ID RESET ---
        if (COOLDOWN_ID != -1 && COOLDOWN_ID < Time.time)
        {
            COOLDOWN_ID = -1;
            if (timers.Count == 0)
            {
                Debug.Log("Cleanup timer ids");
                ID = 0;
            }
        }
        // ------------
        if (timers == null || timers.Count <= 0) return;
        foreach (util_timer timer in timers.Values.ToList())
        {
            if (timer != null) timer.tick();
            else timers.Remove(timer._id);
        }
    }

    public static util_timer simple(float delay, Action func)
    {
        return create(1, delay, (int ticks) => { func.Invoke(); });
    }

    public static util_timer create(int reps, float delay, Action<int> func, Action complete = null)
    {
        util_timer t = new util_timer
        {
            _iterations = reps,
            _delay = delay,
            _func = func,
            _id = (ID++).ToString(),
            _infinite = reps < 0,
            _onComplete = complete
        };

        t.start();
        return t;
    }

    public static void clear()
    {
        foreach (util_timer timer in timers.Values.ToList())
            if (timer != null) timer.stop();

        timers.Clear();
        ID = 0;
    }

    public void tick()
    {
        float currTime = Time.time;

        if (this._paused) return;
        if (currTime < this._nextTick) return;

        if (!this._infinite) this._iterations--;
        if (this._func != null)
        {
            try
            {
                this._func.Invoke(this._iterations);
            }
            catch (Exception err) { Debug.LogError($"[util_timer] ERR: {err.Message}"); }
        }

        if (this._iterations == 0)
        {
            if (this._onComplete != null) this._onComplete.Invoke();
            this.stop();
        }
        else this._nextTick = currTime + this._delay;
    }

    public void setPaused(bool pause)
    {
        this._paused = pause;

        if (pause)
        {
            this._pausedTime = Time.time;
        }
        else
        {
            this._nextTick += (Time.time - this._pausedTime);
            this._pausedTime = 0;
        }
    }

    public void stop()
    {
        if (!timers.ContainsKey(this._id)) return;
        timers.Remove(this._id);

        if (timers.Count == 0)
        {
            COOLDOWN_ID = Time.time + 8;
        }
    }

    public void start()
    {
        if (timers.ContainsKey(this._id)) throw new Exception("Timer already started");
        this._nextTick = Time.time + this._delay;

        timers.Add(this._id, this);
    }

#if DEVELOPMENT_BUILD || UNITY_EDITOR
    public static string debug()
    {
        string data = "\n--------------- ACTIVE TIMERS: " + timers.Count + " | ID : " + ID;

        foreach (util_timer timer in timers.Values.ToList())
        {
            data += "\n [" + timer._id + "] DELAY: " + timer._delay + " | ITERATIONS: " + timer._iterations + " | PAUSED: " + timer._paused + " | TIME: " + (timer._nextTick - Time.time) + "s";
        }

        return data;
    }
#endif
}

# MIT License Copyright (c) 2024 FailCake

# Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the
# "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish,
# distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to
# the following conditions:
#
# The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
#
# THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
# MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR
# ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
# SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.