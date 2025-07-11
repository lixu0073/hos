using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Hospital;
using System.Linq;
//[System.Serializable]
//public class MedicineInfo
//{
//	//public MedicineType Type;
//	public string Name;
//	public Sprite image;

//	public int defaultAmount = 2;
//	public float productionTime = 30;
//	public List<MedicinePrerequisite> prerequisites;
//}

[System.Serializable]
public class MedicinePermutations
{
    private List<MedicineRef> permutation = new List<MedicineRef>();

    public int PermutationSize
    {
        get {
            if (this.permutation != null)
            {
                return this.permutation.Count;
            }
            else return 0;
        }
        private set { }
    }


    public bool HasMedicineOnActualLevel
    {
        get
        {
            if (this.permutation != null)
            {
                for (int i = 0; i < permutation.Count; i++)
                {
                    if (ResourcesHolder.Get().GetMedicineInfos(permutation[i]).minimumLevel == Game.Instance.gameState().GetHospitalLevel())
                        return true;
                }
            }

            return false;
        }
        private set { }
    }

    public MedicinePermutations(List<MedicineRef> perms)
    {
        this.permutation = perms.GetRange(0, perms.Count);
    }

    public MedicinePermutations(MedicineRef perm)
    {
        this.permutation.Add(perm);
    }

    public MedicinePermutations(List<MedicineDatabaseEntry> perms)
    {
        if (perms != null)
        {
            for (int i = 0; i< perms.Count; i++)
            {
                this.permutation.Add(perms[i].GetMedicineRef());
            }
        }
    }

    public bool Contains(List<MedicineRef> list)
    {
        if (list != null && this.permutation!=null)
        {
            for (int i=0; i<list.Count; i++)
            {
                if (i >= this.permutation.Count)
                    break;
                else if (list[i].id == this.permutation[i].id && list[i].type == this.permutation[i].type)
                    return true;
            }
        }
        return false;
    }

    public bool Contains(MedicineRef med)
    {
        if (med != null && this.permutation != null)
        {
            for (int i = 0; i < permutation.Count; i++)
            {
               if (med.id == this.permutation[i].id && med.type == this.permutation[i].type)
                    return true;
            }
        }
        return false;
    }

    public MedicineRef GetMedicineRef(int index)
    {
        return permutation[index];
    }

    public override string ToString()
    {
        StringBuilder builder = new StringBuilder();
        if (permutation!=null)
        {
            for (int i = 0; i < permutation.Count; i++)
            {
                builder.Append(permutation[i].ToString());
                if (i < permutation.Count - 1)
                    builder.Append(";");
            }
        }
        return builder.ToString();
    }


    public static MedicinePermutations Parse(string str)
    {
        List<MedicineRef> tmpMed = new List<MedicineRef>();

        if (!string.IsNullOrEmpty(str))
        {
            var tmp = str.Split(';');
            for (int i = 0; i < tmp.Length; ++i)
                tmpMed.Add(MedicineRef.Get(tmp[i]));
        }

        return new MedicinePermutations(tmpMed); 
    }

    public static int Factorial(int i)
    {
        if (i < 1)
            return 1;
        else
            return i * Factorial(i - 1);
    }

    public static IEnumerable<int[]> Combinations(int k, int n)
    {
        var result = new int[k];
        var stack = new Stack<int>();
        stack.Push(1);

        while (stack.Count > 0)
        {
            var index = stack.Count - 1;
            var value = stack.Pop();

            while (value <= n)
            {
                result[index++] = value++;
                stack.Push(value);
                if (index == k)
                {
                    yield return result;
                    break;
                }
            }
        }
    }
}

