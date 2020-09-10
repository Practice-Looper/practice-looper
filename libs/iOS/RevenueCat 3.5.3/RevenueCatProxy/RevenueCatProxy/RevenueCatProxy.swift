//
//  RevenueCatProxy.swift
//  RevenueCatProxy
//
//  Created by Maksim Kolesnik on 13.09.20.
//  Copyright © 2020 Emka3 Maksim Kolesnik. All rights reserved.
//

import Foundation
import Purchases

@objc(RevenueCatProxy)
public class RevenueCatProxy : NSObject {
    
    @objc
    public func configure(apiKey: String, debug: Bool){
        Purchases.debugLogsEnabled = debug
        Purchases.configure(withAPIKey: apiKey)
    }
    
    @objc
    public func fetchOfferings(){
        Purchases.shared.offerings { (offerings, error) in
            if let packages = offerings?.current?.availablePackages {
                // Display packages for sale
            }
        }
    }
}
