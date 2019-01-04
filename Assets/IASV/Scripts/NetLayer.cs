using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NeuralNetwork;
using UnityEngine.UI;
using System.Threading;
using BodyTalkStorage;
public class NetLayer : MonoBehaviour {

	//Neural Network Variables
	private const double MinimumError = 0.1;
	private const TrainingType TrType = TrainingType.MinimumError;
	private static NeuralNet net;
	public static List<DataSet> dataSets; 
	public static bool trained;

	private int collectedDatasets = 0;
	private const int maxNumberOfDatasets = 50; // 10 neutral, 10 joy, 10 fear, 10 anger, 10 sadness

	public ExpressiveFeaturesExtraction mExpressiveFeatures; 

	// Use this for initialization
	void Start () {
		//Initialize the network 
		net = new NeuralNet(5, 6, 4);
		
		dataSets = (List<DataSet>) StorageHandler.LoadData("new_expressions_data"); // latest: "saved_Data"
		if(dataSets == null){
			dataSets = new List<DataSet>();
		} else{
			collectedDatasets = maxNumberOfDatasets;
		}
	}
	
	// Update is called once per frame
	void Update () {
		//Let the network decide if the player should jump
		// if (trained) {
		// 	double result = compute (new double[]{ player.distanceInPercent, player.canJump });
		// 	if (result > 0.5) {
		// 		player.jump (); 
		// 	}
		// }
	}

	public void Train(double joy, double fear, double anger, double sadness){ 
		double[] C = {(double)mExpressiveFeatures.mFeatureEnergy, 
						(double) mExpressiveFeatures.mFeatureSymmetrySpatial,
						(double) mExpressiveFeatures.mFeatureSymmetrySpread,
						// (double) mExpressiveFeatures.mFeatureSmoothnessLeftHand,
						// (double) mExpressiveFeatures.mFeatureSmoothnessRightHand,
						(double) mExpressiveFeatures.mFeatureSpatialExtent,
						(double) mExpressiveFeatures.mFeatureHeadLeaning
						};

		double[] v = {joy, fear};

		dataSets.Add(new DataSet(C, v));

		collectedDatasets++;
		
		if (!trained && collectedDatasets >= maxNumberOfDatasets) {
			print ("Start training of the network."); 

			StorageHandler.SaveData(dataSets, "new_expressions_data");

			TrainNetwork();
		} else{ // Update emotion to be trained
			switch(collectedDatasets){
				case (10):
				case (20):
                case (30):
                case (40):
                    mExpressiveFeatures.NextEmotion();
				break;
			}
		}
	}

	public double[] compute(double[] vals)
	{
		double[] result = net.Compute(vals);
		return result;
	}

	public static void TrainNetwork()
	{
		net.Train(dataSets, MinimumError);
		trained = true;
		print ("Trained!"); 
	}

	public void DeleteDatabase(){
		StorageHandler.DeleteFile("new_expressions_data");
	}
}
