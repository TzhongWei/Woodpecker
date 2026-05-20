using System;
using Grasshopper.Kernel;
using Woodpecker.Animation.Control.Timeline;
using Woodpecker.Animation.CodeManager;

namespace Woodpecker.Animation.GHComponents.CustomGHComponents
{
    [Obsolete]
    /// <summary>
    /// Remote Time Spot Abstract component.
    /// </summary>
    public abstract class GH_RemoteTimeSpotAbstract : GH_TimelineAbstract, ISingletonDocumentComponent
    {
        public override GH_Exposure Exposure => GH_Exposure.hidden;
        /// <summary>
        /// Check the tag for input or output, 
        /// for all input, the tag should be unique, true -> is unique, false -> is duplicated ||
        /// for all ouput, the tag should be exist,  true -> is exis, false -> isn't defined 
        /// </summary>
        /// <returns></returns>
        public abstract bool IsPrimaryInstance();
        public string SingletonTag { get; protected set; }
        public RemoteTime RemoteTimeData { get; protected set; }
        public GH_RemoteTimeSpotAbstract(string Name, string NickName, string Description) : base(Name, NickName, Description) { }
        protected override string ShowTimeSetupDescription()
        {
            return this.SingletonTag + ": " + $"{RemoteTimeData.Value}";
        }
        public abstract RemoteType _remoteType {get;}
        protected override void AfterSolveInstance()
        {
            if(!IsPrimaryInstance())
            {
                if(this._remoteType == RemoteType.Input)
                {
                    this.AddRuntimeMessage(Grasshopper.Kernel.GH_RuntimeMessageLevel.Error, $"The tag {SingletonTag} is exist, which cannot be named again");
                }
                if(this._remoteType == RemoteType.Output)
                {
                    this.AddRuntimeMessage(Grasshopper.Kernel.GH_RuntimeMessageLevel.Error, $"The tag {SingletonTag} is not defined, please try different tags");
                }
            }
        }
    }
}