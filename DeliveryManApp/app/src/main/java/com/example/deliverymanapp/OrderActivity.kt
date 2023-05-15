package com.example.deliverymanapp

import android.os.Bundle
import androidx.appcompat.app.AppCompatActivity

class OrderActivity : AppCompatActivity() {
    private var orderId : String = ""

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        setContentView(R.layout.activity_order)

        val arguments = intent.extras

        if (arguments != null) {
            orderId = arguments["orderId"] as String

        }
    }
}