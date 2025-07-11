using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hospital
{
    namespace TreatmentRoomHelpRequest
    {
        namespace Backend
        {
            public class TreatmentRoomHelpMapper
            {

                private static List<TreatmentHelpPackage> packages = new List<TreatmentHelpPackage>();

                public static List<TreatmentHelpPackage> Map(List<TreatmentHelpPackageModel> packagesModels, List<TreatmentHelpCureModel> helpersModels)
                {
                    packages.Clear();
                    for (int packageIndex = 0; packageIndex < packagesModels.Count; ++packageIndex)
                    {
                        List<TreatmentHelpCure> helpers = new List<TreatmentHelpCure>();
                        for(int helperIndex = 0; helperIndex < helpersModels.Count; ++helperIndex)
                        {
                            if(packagesModels[packageIndex].SaveID == helpersModels[helperIndex].SaveID && packagesModels[packageIndex].PatientID == helpersModels[helperIndex].PatientID)
                            {
                                helpers.Add(Map(helpersModels[helperIndex]));
                            }
                        }
                        packages.Add(new TreatmentHelpPackage(
                            packagesModels[packageIndex].PatientID,
                            packagesModels[packageIndex].SaveID,
                            ParseMedicinesAmount(packagesModels[packageIndex].UnparsedMedicinesGoals),
                            helpers,
                            packagesModels[packageIndex].FriendsIds
                            ));
                    }
                    return packages;
                }

                #region Packages

                public static TreatmentHelpPackage Map(TreatmentHelpPackageModel model)
                {
                    return new TreatmentHelpPackage(model.PatientID, model.SaveID, ParseMedicinesAmount(model.UnparsedMedicinesGoals), null, model.FriendsIds);
                }

                public static TreatmentHelpPackageModel Map(TreatmentHelpPackage entitiy)
                {
                    return new TreatmentHelpPackageModel()
                    {
                        SaveID = entitiy.SaveID,
                        PatientID = entitiy.PatientID,
                        UnparsedMedicinesGoals = StringifyMedicinesAmount(entitiy.MedicinesGoals),
                        FriendsIds = entitiy.FriendsSavesIDs
                    };
                } 

                #endregion

                #region Helpers

                public static TreatmentHelpCure Map(TreatmentHelpCureModel model)
                {
                    return new TreatmentHelpCure(model.ID, model.PatientID, model.SaveID, new MedicineAmount(model.Medicine, model.Amount), model.HelperSaveID, model.IsFbFriend, model.SendPush);
                }

                public static TreatmentHelpCureModel Map(TreatmentHelpCure entity)
                {
                    return new TreatmentHelpCureModel()
                    {
                        SaveID = entity.SaveID,
                        ID = entity.ID,
                        PatientID = entity.PatientID,
                        Medicine = entity.MedicineInfo.medicine,
                        Amount = entity.MedicineInfo.amount,
                        HelperSaveID = entity.HelperSaveID,
                        IsFbFriend = entity.IsFbFriend,
                        SendPush = entity.SendPush
                    };
                }

                #endregion

                #region MedicineAmmount

                private static List<MedicineAmount> ParseMedicinesAmount(string unparsedMedicinesAmount)
                {
                    List<MedicineAmount> list = new List<MedicineAmount>();
                    string[] unparsedMedicinesAmountArray = unparsedMedicinesAmount.Split(';');
                    for(int i = 0; i < unparsedMedicinesAmountArray.Length; ++i)
                    {
                        if (!string.IsNullOrEmpty(unparsedMedicinesAmountArray[i]))
                        {
                            string[] unparsedMedicineAmount = unparsedMedicinesAmountArray[i].Split(',');
                            MedicineRef medicine = MedicineRef.Parse(unparsedMedicineAmount[0]);
                            int amount = int.Parse(unparsedMedicineAmount[1], System.Globalization.CultureInfo.InvariantCulture);
                            if(medicine != null)
                                list.Add(new MedicineAmount(medicine, amount));
                        }
                    }
                    return list;
                }

                public static string StringifyMedicinesAmount(List<MedicineAmount> medicinesAmount)
                {
                    System.Text.StringBuilder builder = new System.Text.StringBuilder();
                    for (int i = 0; i < medicinesAmount.Count; ++i)
                    {
                        builder.Append(medicinesAmount[i].medicine.ToString());
                        builder.Append(",");
                        builder.Append(medicinesAmount[i].amount);
                        builder.Append(";");
                    }
                    return builder.ToString();
                }

                #endregion

                
            }
        }
    }
}
