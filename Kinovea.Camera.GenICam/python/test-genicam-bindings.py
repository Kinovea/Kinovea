from genicam.gentl import DEVICE_ACCESS_FLAGS_LIST
from genicam.gentl import GenTLProducer, Port
from genicam.gentl import ClosedException, InvalidHandleException
from genicam.genapi import NodeMap, AbstractPort, EAccessMode

import os
import sys
path_name = os.path.join('G:/softs/Program Files/iCentral/iCentral/Runtime/x64', 'MVProducerU3V.cti')
#path_name = os.path.join('G:/softs/Program Files/GalaxySDK/GenTL/Win64', 'GxU3VTL.cti')
#path_name = os.path.join('G:/softs/Program Files/Basler/Runtime/x64', 'ProducerU3V.cti')

producer = GenTLProducer.create_producer()
producer.open(path_name)

# Having called the open() method, the GenTLProducer object will open the specified CTI file and eventually call GCInitLib() function internally. 
# It will allow you to use the funtionality that the target GenTL Producer provides.

system = producer.create_system()
system.open()

system.update_interface_info_list(1000)

interface = system.interface_info_list[0].create_interface()
interface.open()

interface.update_device_info_list(1000)

device = interface.device_info_list[1].create_device()

device.open(
    DEVICE_ACCESS_FLAGS_LIST.DEVICE_ACCESS_EXCLUSIVE
)

#...

producer.close()
