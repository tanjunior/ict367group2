using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FullCarPark : MonoBehaviour
{
    [SerializeField] private float lotWidth = 2f, lotDepth = 4f, parallelLotWidth = 2.8f, parallelLotDepth = 2.3f, lineThickness = 0.2f;
    [SerializeField] private int numberOfLots, parkingSpotIndex;
    [SerializeField] private GameObject parkingSpotPefab, linePefab;
    private List<GameObject> parkingLots = new List<GameObject>();
    private float currentX = 0;
    private float currentY = 0;
    private float currentZ = 0;

    // Start is called before the first frame update
    void Start()
    {
        ParallelParking();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void ParallelParking() {
        for (int i = 0; i < numberOfLots; i++) 
        {
            GameObject lot = new GameObject();
            lot.name = "lot";
            lot.transform.parent = transform;

            GameObject mainLine = Instantiate(linePefab, Vector3.zero, Quaternion.identity);
            mainLine.transform.parent = lot.transform; // have to parent it before changing the transform
            mainLine.transform.localPosition = new Vector3(currentX, 0, currentZ);
            mainLine.transform.localScale = new Vector3(lineThickness, 1, parallelLotWidth-lineThickness);

            GameObject sideLine = Instantiate(linePefab, Vector3.zero, Quaternion.identity);
            sideLine.transform.parent = lot.transform; // have to parent it before changing the transform
            sideLine.transform.localPosition = new Vector3(parallelLotDepth/2-lineThickness/2, 0, currentZ+parallelLotWidth/2);
            sideLine.transform.localScale = new Vector3(parallelLotDepth, 1, lineThickness); // scale in different direction so we don't have to rotate
            

            // GameObject sideLine2 = Instantiate(linePefab, new Vector3(lotDepth/2, 0, -lotWidth/2), Quaternion.identity);
            // sideLine2.transform.parent = lot.transform; // have to parent it before changing the transform
            // sideLine2.transform.localScale = new Vector3(lotDepth, lineThickness, 1); // scale in different direction so we don't have to rotate
            // sideLine2.transform.localRotation = Quaternion.Euler(0, 0, 0); // reset the rotation after parenting
            
            if (i == parkingSpotIndex) {
                GameObject spot = Instantiate(parkingSpotPefab, new Vector3(1.48f, 0, currentZ+0.13f), Quaternion.identity);
            }
            currentZ += parallelLotWidth;
            //currentY += lotDepth;
        }
    }

    private void NormalParking() {
        for (int i = 0; i < numberOfLots; i++) 
        {
            GameObject lot = new GameObject();
            lot.name = "lot";
            lot.transform.parent = transform;
            lot.transform.localRotation = Quaternion.Euler(90, 0, 0); //rotate the container first so don't have to rotate the children

            GameObject mainLine = Instantiate(linePefab, Vector3.zero, Quaternion.identity);
            mainLine.transform.parent = lot.transform; // have to parent it before changing the transform
            mainLine.transform.localPosition = new Vector3(currentX, currentY, 0);
            mainLine.transform.localScale = new Vector3(lotWidth-lineThickness, lineThickness, 1);
            mainLine.transform.localRotation = Quaternion.Euler(0, 0, 0); // reset the rotation after parenting

            GameObject sideLine = Instantiate(linePefab, Vector3.zero, Quaternion.identity);
            sideLine.transform.parent = lot.transform; // have to parent it before changing the transform
            sideLine.transform.localPosition = new Vector3(currentX+lotWidth/2, lotDepth/2-lineThickness/2, 0);
            sideLine.transform.localScale = new Vector3(lineThickness, lotDepth, 1); // scale in different direction so we don't have to rotate
            sideLine.transform.localRotation = Quaternion.Euler(0, 0, 0); // reset the rotation after parenting
            

            // GameObject sideLine2 = Instantiate(linePefab, new Vector3(lotDepth/2, 0, -lotWidth/2), Quaternion.identity);
            // sideLine2.transform.parent = lot.transform; // have to parent it before changing the transform
            // sideLine2.transform.localScale = new Vector3(lotDepth, lineThickness, 1); // scale in different direction so we don't have to rotate
            // sideLine2.transform.localRotation = Quaternion.Euler(0, 0, 0); // reset the rotation after parenting
            
            if (i == parkingSpotIndex) {
                GameObject spot = Instantiate(parkingSpotPefab, new Vector3(currentX, currentY, 2.6f), Quaternion.identity);
            }
            currentX += lotWidth;
            //currentY += lotDepth;
        }
    }
}
