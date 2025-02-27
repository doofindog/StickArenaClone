using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class NetInputProcessor : NetworkBehaviour
{
    private const int NETWORK_BUFFER_SIZE = 1024;

    private int m_processedSequenceNumber = -1;
    private int m_sequenceNumber = 0;
    private NetInputPayLoad[] m_inputPayLoads = new NetInputPayLoad[NETWORK_BUFFER_SIZE];
    private List<NetInputPayLoad> m_inputsQueue = new List<NetInputPayLoad>(); // For server processing / client confirmation

    public Action<NetInputPayLoad> processedInputEvent;


    private void Awake()
    {
        //Initialise and mark all payload slots as null
        for (int i = 0; i < m_inputPayLoads.Length; i++)
        {
            m_inputPayLoads[i] = new NetInputPayLoad();
            m_inputPayLoads[i].isNull = true;
        }
    }

    [ServerRpc(Delivery = RpcDelivery.Unreliable)]
    private void SendInputServerRPC(NetInputPayLoad inputPayLoad)
    {
        // Enqueue input for processing on the server
        m_inputsQueue.Add(inputPayLoad);
    }

    // Retrieve previous payloads to handle missing input sequences
    private List<NetInputPayLoad> GetPreviousPayloads(NetInputPayLoad pPayLoad, int count)
    {
        List<NetInputPayLoad> prePayloads = new List<NetInputPayLoad>();
        int bufferIndex = pPayLoad.payloadSequence % NETWORK_BUFFER_SIZE;
        for (int i = 0; i < count; i++)
        {
            bufferIndex--;
            if (bufferIndex < 0)
            {
                continue;
            }

            NetInputPayLoad prevInputPayload = m_inputPayLoads[bufferIndex];
            if (prevInputPayload.isNull)
            {
                continue;
            }

            prePayloads.Add(new NetInputPayLoad()
            {
                isNull = prevInputPayload.isNull,
                payloadSequence = prevInputPayload.payloadSequence,
                time = prevInputPayload.time,
                tick = prevInputPayload.tick,
                mousePosition = prevInputPayload.mousePosition,
                direction = prevInputPayload.direction,
                aimAngle = prevInputPayload.aimAngle,
            });
        }

        prePayloads.Sort((a, b) => a.payloadSequence.CompareTo(b.payloadSequence));
        return prePayloads;
    }

    public NetInputPayLoad AddInput(NetInputPayLoad inputPayLoad)
    {
        // Use modulo of external NETWORK_BUFFER_SIZE to determine buffer index
        int bufferIndex = m_sequenceNumber % NETWORK_BUFFER_SIZE;

        m_inputPayLoads[bufferIndex] = inputPayLoad;
        m_inputPayLoads[bufferIndex].isNull = false;
        m_inputPayLoads[bufferIndex].payloadSequence = m_sequenceNumber;
        m_inputPayLoads[bufferIndex].previousPayloads = GetPreviousPayloads(m_inputPayLoads[bufferIndex], 5);

        // Send the payload to the server
        SendInputServerRPC(m_inputPayLoads[bufferIndex]);

        m_sequenceNumber++;
        return m_inputPayLoads[bufferIndex];
    }


    // Process queued inputs and manage missing sequences
    public NetInputPayLoad[] ProcessInputs()
    {
        if (m_inputsQueue.Count == 0)
            return Array.Empty<NetInputPayLoad>();

        List<NetInputPayLoad> sortedPayloads = new List<NetInputPayLoad>();
        HashSet<int> processedInputs = new HashSet<int>();

        m_inputsQueue.Sort((a, b) => a.payloadSequence.CompareTo(b.payloadSequence));
        if(m_processedSequenceNumber == -1)
        {
            m_processedSequenceNumber = m_inputPayLoads[0].payloadSequence - 1;
        }

        foreach (NetInputPayLoad currentPayload in m_inputsQueue)
        {
            if (currentPayload.payloadSequence < m_processedSequenceNumber)
            {
                continue;
            }

            int expectedSequence = m_processedSequenceNumber + 1;
            if (currentPayload.payloadSequence != expectedSequence)
            {
                int missingCount = Mathf.Clamp(currentPayload.payloadSequence - expectedSequence, 0 , currentPayload.previousPayloads.Count - 1);
                int startIndex = (currentPayload.previousPayloads.Count - missingCount);
                for (int i = startIndex; i < currentPayload.previousPayloads.Count; i++)
                {
                    if (processedInputs.Contains(currentPayload.previousPayloads[i].payloadSequence))
                    {
                        continue;
                    }
                    sortedPayloads.Add(currentPayload.previousPayloads[i]);
                    processedInputs.Add(currentPayload.previousPayloads[i].payloadSequence);
                }
            }

            sortedPayloads.Add(currentPayload);
            m_processedSequenceNumber = currentPayload.payloadSequence;
        }

        m_inputsQueue.Clear();
        processedInputs.Clear();

        return sortedPayloads.ToArray();
    }

    // Retrieve payload based on sequence value
    public NetInputPayLoad GetPayloadAtSequence(int sequence)
    {
        int index = sequence % NETWORK_BUFFER_SIZE;
        return m_inputPayLoads[index];
    }

    public int GetCurrentSequenceCount()
    {
        return m_processedSequenceNumber;
    }
}
