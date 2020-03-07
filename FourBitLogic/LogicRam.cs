using KSerialization;

namespace Egladil
{
    [SerializationConfig(MemberSerialization.OptIn)]
    public class LogicRam : LogicBase
    {
        [Serialize]
        private byte[] memory;

        [Serialize]
        private bool oldWrite;

        public LogicCircuitNetwork DataNetwork => Game.Instance.logicCircuitManager.GetNetworkForCell(ports.GetPortCell(FourBitLogic.DATA));

        protected override void OnSpawn()
        {
            base.OnSpawn();
            if (memory == null)
            {
                memory = new byte[256];
            }
        }


        protected override void OnLogicValueChanged(LogicValueChanged data)
        {
            if (data.portID == FourBitLogic.DATA) return;

            bool read = (ports.GetInputValue(FourBitLogic.READ) & 0x1) == 0x1;
            bool write = (ports.GetInputValue(FourBitLogic.WRITE) & 0x1) == 0x1;

            int address1 = ports.GetInputValue(FourBitLogic.ADDRESS_1) & 0xf;
            int address2 = ports.GetInputValue(FourBitLogic.ADDRESS_2) & 0xf;
            int address = address1 | (address2 << 4);

            Log.Spam($"R: {read}, W: {oldWrite} -> {write}, A: {address}");

            int dataOut = read ? memory[address] : 0;
            ports.SendSignal(FourBitLogic.DATA, dataOut);
            Log.Spam($"Read: {dataOut}");

            if (write && !oldWrite)
            {
                int dataIn = DataNetwork.OutputValue & 0xf;
                memory[address] = (byte)dataIn;
                Log.Spam($"Write: {dataIn}");
            }

            oldWrite = write;
            
            KBatchedAnimController kbac = GetComponent<KBatchedAnimController>();
            if (kbac != null)
            {
                int value = (read ? 1 : 0) | (write ? 2 : 0);
                kbac.Play($"on_{value}");
            }
        }
    }
}
